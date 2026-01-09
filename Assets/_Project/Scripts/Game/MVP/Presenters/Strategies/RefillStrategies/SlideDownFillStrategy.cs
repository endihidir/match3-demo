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
        }

        private bool SpawnTopOpenSegment(GridStateContext context, IGridView view, int x, int height)
        {
            var model = context.Model;
            var cellSize = view.GetCellSize();

            var segmentBlocked = false;
            var spawnedAny = false;

            var spawnY = 0f;
            var hasSpawnY = false;

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                if (!model.IsCellActive(c))
                {
                    segmentBlocked = false;
                    continue;
                }

                var obj = model.GetGridObject(c);

                if (obj)
                {
                    segmentBlocked = true;
                    continue;
                }

                if (segmentBlocked) continue;

                if (!hasSpawnY)
                {
                    spawnY = view.GridToWorld(c).y + cellSize;
                    hasSpawnY = true;
                }

                var type = SmartSpawnDecider.Decide(model, c, _settings.SpawnSettings, 0f);
                var item = context.Factory.GetRegularItem(type);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var w = view.GridToWorld(c);
                item.SetPosition(new Vector3(w.x, spawnY, w.z));

                model.SetGridObject(c, item);

                AddStep(item, c, true);
                spawnedAny = true;
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

                var final = path[^1];
                var isSlide = path[0].x != final.x;

                _moves.Add(new SlideDownMoveRecord(item, path, final, track.IsSpawn, isSlide));
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

                var finalX = moveRecord.Final.x;
                ref var state = ref _waveByX[finalX];

                int delayIndex;

                if (moveRecord.IsSlide)
                {
                    if (moveRecord.IsSpawn)
                    {
                        delayIndex = state.SpawnSlide;
                        state.SpawnSlide++;
                    }
                    else
                    {
                        delayIndex = state.Slide;
                        state.Slide++;
                        state.NonSpawnSlideCount++;
                    }
                }
                else
                {
                    if (moveRecord.IsSpawn)
                    {
                        delayIndex = state.NonSpawnSlideCount + state.SpawnFall;
                        state.SpawnFall++;
                    }
                    else
                    {
                        delayIndex = state.Fall;
                        state.Fall++;
                    }
                }

                var length = moveRecord.Path.Count + (moveRecord.IsSpawn ? 1 : 0);
                var world = ArrayPool<Vector3>.Shared.Rent(length);

                int filled = 0;
                if (moveRecord.IsSpawn)
                    world[filled++] = moveRecord.Item.transform.position;

                for (int p = 0; p < moveRecord.Path.Count; p++)
                    world[filled++] = view.GridToWorld(moveRecord.Path[p]);

                var delay = delayIndex * _settings.ShiftDelayMultiplier;

                var totalDuration = 0f;
                var current = moveRecord.Item.transform.position;

                for (int w = 0; w < length; w++)
                {
                    var next = world[w];
                    var distCells = Mathf.Abs(current.y - next.y) / cellSize;
                    var segMul = 1f + distCells * _settings.ShiftDurationMultiplier;
                    totalDuration += segMul;
                    current = next;
                }

                var tween = moveRecord.Item.ItemAnimation.ShiftPath(world, length, totalDuration, delay);

                _animTasks[taskCount++] = WaitTweenAndReturnArray(tween, world);
            }

            return taskCount == 0 ? Task.CompletedTask : Task.WhenAll(_animTasks.AsSpan(0, taskCount).ToArray());
        }

        private static async Task WaitTweenAndReturnArray(Tween tween, Vector3[] rented)
        {
            try
            {
                await tween.AsyncWaitForCompletion();
            }
            finally
            {
                ArrayPool<Vector3>.Shared.Return(rented);
            }
        }
    }
}