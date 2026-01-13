using System;
using System.Collections.Generic;
using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Models;
using Core.Pool;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideDownFillStrategy : IFillStrategy
    {
        private sealed class SlideDownTrack
        {
            public readonly List<Vector2Int> Path = new(8);
            public bool IsSpawn;
        }

        private readonly FillStrategySettingsSO _strategySettings;
        private readonly IFillItemDecider _itemDecider;
        private readonly Dictionary<BaseGridObject, SlideDownTrack> _trackByItem = new(256);
        private readonly List<SlideDownCandidate> _candidates = new(128);
        private readonly List<SlideDownMoveRecord> _moves = new(256);

        private SlideDownColumnWaveState[] _waveByX = Array.Empty<SlideDownColumnWaveState>();
        private int[] _usedTargetStamp = Array.Empty<int>();
        private int[] _spawnStackByX = Array.Empty<int>();
        private int[] _usedSourceStamp = Array.Empty<int>();
        private int _usedSourceStampId = 0;
        private int _usedTargetStampId = 1;

        private UniTask[] _animTasks = new UniTask[128];
        private UniTask _runningAnimations = UniTask.CompletedTask;

        public SlideDownFillStrategy(GameplayConfigContainer config, IFillItemDecider itemDecider)
        {
            _strategySettings = config.FillStrategySettings;
            _itemDecider = itemDecider;
        }

        public bool CanHandle(IGridModel model) => GridFillCalcUtil.HasStationaryAndBlocking(model);

        public IFillStrategy Execute(GridStateContext context)
        {
            _trackByItem.Clear();

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            EnsureBuffers(width, height);
            
            var movedAny = true;

            while (movedAny)
            {
                movedAny = false;

                for (int x = 0; x < width; x++)
                {
                    for (int y = height - 1; y >= 0; y--)
                    {
                        var dst = new Vector2Int(x, y);

                        if (!GridFillCalcUtil.IsEmptyActiveCell(model, dst)) continue;
                        if (!GridFillCalcUtil.CanFallVertically(model, x, y, out var src)) continue;

                        var item = model.GetGridObject(src);
                        if (!item || item.IsStationary) continue;

                        model.SetGridObject(src, null);
                        model.SetGridObject(dst, item);

                        AddStep(item, dst, false);
                        movedAny = true;
                    }
                }

                _candidates.Clear();

                for (int y = height - 1; y >= 1; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var sourceCoord = new Vector2Int(x, y);

                        if (!GridFillCalcUtil.IsEmptyActiveCell(model, sourceCoord)) continue;
                        if (GridFillCalcUtil.CanFallVertically(model, x, y, out _)) continue;
                        if (!GridFillCalcUtil.HasStationaryAboveInSameSegment(model, sourceCoord)) continue;

                        if (GridFillCalcUtil.TryCollectDiagonalSide(model, sourceCoord, 1, out var right))
                            _candidates.Add(right);

                        if (GridFillCalcUtil.TryCollectDiagonalSide(model, sourceCoord, -1, out var left))
                            _candidates.Add(left);
                    }
                }

                _usedTargetStampId++;
                _usedSourceStampId++;

                if (_usedTargetStampId == int.MaxValue || _usedSourceStampId == int.MaxValue)
                {
                    Array.Clear(_usedTargetStamp, 0, _usedTargetStamp.Length);
                    Array.Clear(_usedSourceStamp, 0, _usedSourceStamp.Length);
                    _usedTargetStampId = 1;
                    _usedSourceStampId = 1;
                }

                for (int i = 0; i < _candidates.Count; i++)
                {
                    var candidate = _candidates[i];
                    
                    if (model.GetGridObject(candidate.From) != candidate.Item) continue;
                    if (model.GetGridObject(candidate.To) != null) continue;

                    var ti = candidate.To.x + candidate.To.y * width;
                    if (_usedTargetStamp[ti] == _usedTargetStampId) continue;
                    _usedTargetStamp[ti] = _usedTargetStampId;

                    var fi = candidate.From.x + candidate.From.y * width;
                    if (_usedSourceStamp[fi] == _usedSourceStampId) continue;
                    _usedSourceStamp[fi] = _usedSourceStampId;

                    model.SetGridObject(candidate.From, null);
                    model.SetGridObject(candidate.To, candidate.Item);

                    AddStep(candidate.Item, candidate.To, false);
                    movedAny = true;
                }
                
                for (int x = 0; x < width; x++)
                {
                    if (SpawnTopOpenSegment(context, view, x, height))
                        movedAny = true;
                }
            }

            _runningAnimations = PlayAnimations(view, width);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        private void EnsureBuffers(int width, int height)
        {
            if (_waveByX.Length < width)
                _waveByX = new SlideDownColumnWaveState[width];

            var cellCount = width * height;
            if (_usedTargetStamp.Length < cellCount)
                _usedTargetStamp = new int[cellCount];
            
            if (_spawnStackByX.Length < width)
                _spawnStackByX = new int[width];
            else
                Array.Clear(_spawnStackByX, 0, width);
            
            if (_usedSourceStamp.Length < cellCount)
                _usedSourceStamp = new int[cellCount];
        }

       private bool SpawnTopOpenSegment(GridStateContext context, IGridView view, int x, int height)
       { 
           var model = context.Model;
           var cellSize = view.GetCellSize();

           var spawnedAny = false;

           var segmentStartY = -1;
           var blockedInSegment = false;
           var segmentTopWorldY = 0f;

           for (int y = 0; y < height; y++)
           {
               var c = new Vector2Int(x, y);

               if (!model.IsCellActive(c))
               {
                   segmentStartY = -1;
                   blockedInSegment = false;
                   continue;
               }

               if (segmentStartY < 0)
               {
                   segmentStartY = y;
                   segmentTopWorldY = view.GridToWorld(new Vector2Int(x, y)).y + cellSize;
               }

               var obj = model.GetGridObject(c);

               if (obj)
               {
                   blockedInSegment = true;
                   continue;
               }

               if (blockedInSegment) continue;

               var spawnCount = GridFillCalcUtil.CountEmptiesDown(model, x, y, height);
               if (spawnCount <= 0) continue;

               SpawnInto(model, context, view, x, y, spawnCount, segmentTopWorldY, cellSize);

               spawnedAny = true;
               y += spawnCount - 1;
            }

            return spawnedAny;
        }

        private void SpawnInto(IGridModel model, GridStateContext context, IGridView view, int x, int startY, int spawnCount, float segmentTopWorldY, float cellSize)
        {
            var baseStack = _spawnStackByX[x];

            for (int i = 0; i < spawnCount; i++)
            {
                var targetCoord = new Vector2Int(x, startY + i);

                var type = _itemDecider.Decide(model, targetCoord);
                var item = context.Factory.GetRegularItem(type);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var w = view.GridToWorld(targetCoord);

                var reverseIndex = (spawnCount - 1) - i;
                var spawnY = segmentTopWorldY + (baseStack + reverseIndex) * cellSize;

                item.SetPosition(new Vector3(w.x, spawnY, w.z));

                model.SetGridObject(targetCoord, item);
                AddStep(item, targetCoord, true);
            }

            _spawnStackByX[x] = baseStack + spawnCount;
        }


        private void AddStep(BaseGridObject item, Vector2Int step, bool isSpawn)
        {
            if (!_trackByItem.TryGetValue(item, out var track))
            {
                track = new SlideDownTrack();
                _trackByItem[item] = track;
            }

            track.IsSpawn |= isSpawn;

            var path = track.Path;
            if (path.Count == 0 || path[^1] != step)
                path.Add(step);
        }

        private UniTask PlayAnimations(IGridView view, int width)
        {
            _moves.Clear();

            foreach (var kv in _trackByItem)
            {
                var item = kv.Key;
                var track = kv.Value;

                if (!item) continue;

                var path = track.Path;
                if (path == null || path.Count == 0) continue;

                var start = view.WorldToGrid(item.transform.position);
                var final = path[^1];
                var isSlide = start.x != final.x;

                _moves.Add(new SlideDownMoveRecord(item, path, start, final, track.IsSpawn, isSlide));
            }

            _moves.Sort((a, b) =>
            {
                if (a.IsSpawn != b.IsSpawn) return a.IsSpawn ? 1 : -1;
                var y = b.Final.y.CompareTo(a.Final.y);
                return y != 0 ? y : a.Final.x.CompareTo(b.Final.x);
            });

            if (_animTasks.Length < _moves.Count)
                Array.Resize(ref _animTasks, _moves.Count);

            Array.Clear(_waveByX, 0, width);

            var cellSize = view.GetCellSize();
            var taskCount = 0;

            for (int i = 0; i < _moves.Count; i++)
            {
                var moveRecord = _moves[i];
                if (!moveRecord.Item) continue;

                if (!moveRecord.IsSlide)
                {
                    var finalX = moveRecord.Final.x;
                    ref var state = ref _waveByX[finalX];

                    var shiftWave = 0;
                    
                    if (moveRecord.IsSpawn)
                    {
                        shiftWave = state.SpawnFall + state.Fall + state.SpawnSlide + state.Slide;
                        state.SpawnFall++;
                    }
                    else
                    {
                        shiftWave = state.Fall;
                        state.Fall++;
                    }
                    
                    var delay = shiftWave * _strategySettings.ShiftDelayMultiplier;

                    var finalWorld = view.GridToWorld(moveRecord.Final);
                    var startWorld = moveRecord.Item.transform.position;

                    var distCells = Mathf.Abs(finalWorld.y - startWorld.y) / cellSize;

                    var tween = moveRecord.Item.ItemAnimation.Shift(finalWorld, distCells, delay);
                    _animTasks[taskCount++] = tween.ToUniTask();
                }
                else
                {
                    var startX = moveRecord.Start.x;
                    ref var state = ref _waveByX[startX];

                    int slideWave;
                    
                    if (moveRecord.IsSpawn)
                    {
                        slideWave = state.SpawnSlide + state.Slide;
                        state.SpawnSlide++;
                    }
                    else
                    {
                        slideWave = state.Slide + state.Fall;
                        state.Slide++;
                    }
                    
                    var delay = slideWave * _strategySettings.SlideDelayMultiplier;

                    var length = moveRecord.Path.Count + (moveRecord.IsSpawn ? 1 : 0);

                    using var pathWorldPoints = new PooledArray<Vector3>(length);
                    using var segmentCellDistances = new PooledArray<float>(length);

                    var filled = 0;

                    if (moveRecord.IsSpawn)
                        pathWorldPoints .Array[filled++] = moveRecord.Item.transform.position;

                    for (int p = 0; p < moveRecord.Path.Count; p++)
                        pathWorldPoints .Array[filled++] = view.GridToWorld(moveRecord.Path[p]);

                    var current = moveRecord.Item.transform.position;

                    for (int s = 0; s < length; s++)
                    {
                        var next = pathWorldPoints .Array[s];
                        segmentCellDistances.Array[s] = Mathf.Abs(next.y - current.y) / cellSize;
                        current = next;
                    }

                    var tween = moveRecord.Item.ItemAnimation.Slide(pathWorldPoints .Array, length, segmentCellDistances.Array, delay);
                    _animTasks[taskCount++] = tween.ToUniTask();
                }
            }

            return taskCount == 0 ? UniTask.CompletedTask : UniTask.WhenAll(_animTasks.AsSpan(0, taskCount).ToArray());
        }
    }
}