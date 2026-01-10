using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Models;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        private readonly RefillSettingsSO _settings;

        private readonly Dictionary<BaseGridObject, SlideDownTrack> _trackByItem = new(256);
        private readonly List<SlideDownCandidate> _candidates = new(128);
        private readonly List<SlideDownMoveRecord> _moves = new(256);

        private SlideDownColumnWaveState[] _waveByX = Array.Empty<SlideDownColumnWaveState>();
        private int[] _usedTargetStamp = Array.Empty<int>();
        private int[] _spawnStackByX = Array.Empty<int>();
        private int _usedTargetStampId = 1;

        private Task[] _animTasks = new Task[128];
        private Task _runningAnimations = Task.CompletedTask;

        public SlideDownFillStrategy(GameplayConfigContainer config)
        {
            _settings = config.RefillSettings;
        }

        public bool CanRefill(IGridModel model) => GridRefillCalcUtil.HasStationaryAndBlocking(model);

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

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, dst)) continue;
                        if (!GridRefillCalcUtil.CanFallVertically(model, x, y, out var src)) continue;

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
                        var target = new Vector2Int(x, y);

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, target)) continue;
                        if (GridRefillCalcUtil.CanFallVertically(model, x, y, out _)) continue;
                        if (!GridRefillCalcUtil.DestinationBlockedByStationaryAbove(model, target)) continue;

                        if (GridRefillCalcUtil.TryCollectDiagonalSide(model, target, 1, out var right))
                            _candidates.Add(right);

                        if (GridRefillCalcUtil.TryCollectDiagonalSide(model, target, -1, out var left))
                            _candidates.Add(left);
                    }
                }

                if (_candidates.Count > 0)
                {
                    _usedTargetStampId++;
                    if (_usedTargetStampId == int.MaxValue)
                    {
                        Array.Clear(_usedTargetStamp, 0, _usedTargetStamp.Length);
                        _usedTargetStampId = 1;
                    }

                    for (int i = 0; i < _candidates.Count; i++)
                    {
                        var c = _candidates[i];

                        var ti = c.To.x + c.To.y * width;
                        if (_usedTargetStamp[ti] == _usedTargetStampId) continue;
                        _usedTargetStamp[ti] = _usedTargetStampId;

                        model.SetGridObject(c.From, null);
                        model.SetGridObject(c.To, c.Item);

                        AddStep(c.Item, c.To, false);
                        movedAny = true;
                    }
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

        public UniTask WaitAnimationsAsync() => _runningAnimations.AsUniTask();

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
        }
        

        private const int ExtraSpawnOffsetCells = 0;

       private bool SpawnTopOpenSegment(GridStateContext context, IGridView view, int x, int height)
        {
            var model = context.Model;
            var cellSize = view.GetCellSize();

            var spawnedAny = false;

            var segmentBlocked = false;
            var segmentStartY = -1;
            var segmentTopWorldY = 0f;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!model.IsCellActive(c))
                {
                    segmentBlocked = false;
                    segmentStartY = -1;
                    continue;
                }

                if (segmentStartY < 0)
                {
                    segmentStartY = y;
                    var topCellWorld = view.GridToWorld(new Vector2Int(x, segmentStartY));
                    segmentTopWorldY = topCellWorld.y + cellSize;
                }

                var obj = model.GetGridObject(c);

                if (obj)
                {
                    segmentBlocked = true;
                    continue;
                }

                if (segmentBlocked) continue;

                int spawnCount = 0;
                for (int scanY = y; scanY < height; scanY++)
                {
                    var sc = new Vector2Int(x, scanY);

                    if (!model.IsCellActive(sc)) break;
                    if (model.GetGridObject(sc)) break;

                    spawnCount++;
                }

                var baseStack = _spawnStackByX[x];

                for (int i = 0; i < spawnCount; i++)
                {
                    var tc = new Vector2Int(x, y + i);

                    var type = SmartSpawnDecider.Decide(model, tc, _settings.SpawnSettings, 0f);
                    var item = context.Factory.GetRegularItem(type);

                    item.SetParent(view.GridObjectsParent);
                    item.SetSpriteSize(cellSize);

                    var targetWorld = view.GridToWorld(tc);

                    var reverseIndex = (spawnCount - 1) - i;
                    var stack = baseStack + reverseIndex + ExtraSpawnOffsetCells;

                    var spawnY = segmentTopWorldY + stack * cellSize;

                    item.SetPosition(new Vector3(targetWorld.x, spawnY, targetWorld.z));

                    model.SetGridObject(tc, item);
                    AddStep(item, tc, true);

                    spawnedAny = true;
                }

                _spawnStackByX[x] = baseStack + spawnCount;

                y += (spawnCount - 1);
            }

            return spawnedAny;
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

        private Task PlayAnimations(IGridView view, int width)
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
            var slideIndex = 0;
            var shiftIndex = 0;

            for (int i = 0; i < _moves.Count; i++)
            {
                var moveRecord = _moves[i];

                if (!moveRecord.IsSlide)
                {
                    var finalX = moveRecord.Final.x;
                    ref var state = ref _waveByX[finalX];

                    if (moveRecord.IsSpawn)
                    {
                        shiftIndex = state.SpawnFall + state.SpawnSlide;
                        state.SpawnFall++;
                    }
                    else
                    {
                        shiftIndex = state.Fall;
                        state.Fall++;
                    }
                }
                else
                {
                    var startX = moveRecord.Start.x;
                    ref var state = ref _waveByX[startX];

                    if (moveRecord.IsSpawn)
                    {
                        slideIndex = state.SpawnSlide + state.Slide;
                        state.SpawnSlide++;
                    }
                    else
                    {
                        slideIndex = state.Slide + state.Fall;
                        state.Slide++;
                    }
                }
            }

            for (int i = 0; i < _moves.Count; i++)
            {
                var moveRecord = _moves[i];
                if (!moveRecord.Item) continue;

                if (!moveRecord.IsSlide)
                {
                    var delay = shiftIndex * _settings.ShiftDelayMultiplier;

                    var finalWorld = view.GridToWorld(moveRecord.Final);
                    var startWorld = moveRecord.Item.transform.position;

                    var distCells = Mathf.Abs(finalWorld.y - startWorld.y) / cellSize;

                    var tween = moveRecord.Item.ItemAnimation.Shift(finalWorld, distCells, delay);
                    _animTasks[taskCount++] = tween.AsyncWaitForCompletion();
                }
                else
                {
                    var delay = slideIndex * _settings.SlideDelayMultiplier;

                    var length = moveRecord.Path.Count + (moveRecord.IsSpawn ? 1 : 0);

                    var world = ArrayPool<Vector3>.Shared.Rent(length);
                    var cells = ArrayPool<float>.Shared.Rent(length);

                    int filled = 0;

                    if (moveRecord.IsSpawn)
                        world[filled++] = moveRecord.Item.transform.position;

                    for (int p = 0; p < moveRecord.Path.Count; p++)
                        world[filled++] = view.GridToWorld(moveRecord.Path[p]);

                    var current = moveRecord.Item.transform.position;

                    for (int s = 0; s < length; s++)
                    {
                        var next = world[s];
                        cells[s] = Mathf.Abs(next.y - current.y) / cellSize;
                        current = next;
                    }

                    var tween = moveRecord.Item.ItemAnimation.Slide(world, length, cells, delay);
                    _animTasks[taskCount++] = WaitTweenAndReturnArrays(tween, world, cells);
                }
            }

            return taskCount == 0 ? Task.CompletedTask : Task.WhenAll(_animTasks.AsSpan(0, taskCount).ToArray());
        }

        private static async Task WaitTweenAndReturnArrays(Tween tween, Vector3[] world, float[] cells)
        {
            try
            {
                await tween.AsyncWaitForCompletion();
            }
            finally
            {
                ArrayPool<Vector3>.Shared.Return(world);
                ArrayPool<float>.Shared.Return(cells);
            }
        }

    }
}