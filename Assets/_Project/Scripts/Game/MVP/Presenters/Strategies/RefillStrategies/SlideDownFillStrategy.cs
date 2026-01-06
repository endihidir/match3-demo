using System.Collections.Generic;
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
    public sealed class SlideDownFillStrategy : IRefillStrategy
    {
        private readonly RefillSettingsSO _settings;

        private readonly Dictionary<BaseGridObject, List<Vector2Int>> _pathByItem = new(256);
        private readonly HashSet<BaseGridObject> _spawned = new(128);
        private readonly List<UniTask> _animTasks = new(128);
        private UniTask _runningAnimations;

        // Reused buffer (segment-local coords)
        private readonly List<Vector2Int> _spawnCoords = new(64);

        public SlideDownFillStrategy(GameplayConfigContainer config)
        {
            _settings = config.ItemConfigContainer.RefillSettingsSo;
        }

        public bool CanRefill(IGridModel model)
        {
            return GridRefillCalcUtil.HasAnyEmptyActiveCell(model);
        }

        public IRefillStrategy Execute(GridStateContext context)
        {
            _pathByItem.Clear();
            _spawned.Clear();

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var movedAny = true;

            while (movedAny)
            {
                movedAny = false;

                // 1) Straight vertical falls
                for (int x = 0; x < width; x++)
                {
                    for (int y = height - 1; y >= 0; y--)
                    {
                        var dst = new Vector2Int(x, y);

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, dst))
                            continue;

                        if (!CanFallVertically(model, x, y, out var src))
                            continue;

                        var item = model.GetGridObject(src);
                        if (!item || item.IsStationary)
                            continue;

                        ApplyMove(model, item, src, dst);
                        movedAny = true;
                    }
                }

                // 2) Diagonal slide candidates
                var slides = new List<SlideMoveRecord>(64);

                for (int y = height - 1; y >= 1; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var empty = new Vector2Int(x, y);

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, empty))
                            continue;

                        // If it can be filled vertically, do not slide into it.
                        if (CanFallVertically(model, x, y, out _))
                            continue;

                        if (!DestinationBlockedByStationaryAbove(model, empty))
                            continue;

                        if (TryCollectDiagonalSlide(model, empty, -1, out var left))
                            slides.Add(left);

                        if (TryCollectDiagonalSlide(model, empty, +1, out var right))
                            slides.Add(right);
                    }
                }

                if (slides.Count > 0)
                {
                    var usedTargets = new HashSet<Vector2Int>();

                    for (int i = 0; i < slides.Count; i++)
                    {
                        var slide = slides[i];

                        if (!usedTargets.Add(slide.To))
                            continue;

                        ApplyMove(model, slide.Item, slide.From, slide.To);
                        movedAny = true;
                    }
                }

                // 3) Spawn into top-open segments (FallDown-like spawnY, but per segment)
                for (int x = 0; x < width; x++)
                {
                    if (SpawnTopOpenSegmentsInColumn(context, view, x, height))
                        movedAny = true;
                }
            }

            _runningAnimations = PlayAnimations(context);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        private bool SpawnTopOpenSegmentsInColumn(GridStateContext context, IGridView view, int x, int height)
        {
            var model = context.Model;
            var cellSize = view.GetCellSize();

            var spawnedAny = false;

            // Segment state
            _spawnCoords.Clear();
            var segmentHasStarted = false;
            var segmentBlockedByObject = false;
            var segmentTop = default(Vector2Int);

            for (int y = 0; y < height; y++)
            {
                var c = new Vector2Int(x, y);

                // Inactive cuts the column into segments
                if (!model.IsCellActive(c))
                {
                    FlushSegment();

                    segmentHasStarted = false;
                    segmentBlockedByObject = false;
                    segmentTop = default;
                    continue;
                }

                if (!segmentHasStarted)
                {
                    segmentHasStarted = true;
                    segmentTop = c;
                }

                var obj = model.GetGridObject(c);

                // Any object blocks further spawns in this segment (we only spawn into the "top-open" part)
                if (obj)
                {
                    FlushSegment();
                    segmentBlockedByObject = true;
                    continue;
                }

                if (segmentBlockedByObject)
                    continue;

                // Empty + top-open => spawn candidate
                _spawnCoords.Add(c);
            }

            FlushSegment();
            return spawnedAny;

            void FlushSegment()
            {
                if (_spawnCoords.Count == 0)
                    return;

                // Spawn from this segment's top (FallDown-style): all spawns start from same spawnY.
                var spawnY = view.GridToWorld(segmentTop).y + cellSize;

                for (int i = 0; i < _spawnCoords.Count; i++)
                {
                    var coord = _spawnCoords[i];

                    var type = SmartSpawnDecider.Decide(model, coord, _settings.SpawnSettings);
                    var item = context.Factory.GetRegularItem(type);

                    item.SetParent(view.GridObjectsParent);
                    item.SetSpriteSize(cellSize);

                    var target = view.GridToWorld(coord);
                    var start = new Vector3(target.x, spawnY, target.z);

                    item.SetPosition(start);

                    model.SetGridObject(coord, item);
                    _spawned.Add(item);

                    AddPath(item, coord);
                }

                _spawnCoords.Clear();
                spawnedAny = true;
            }
        }

        private static bool CanFallVertically(IGridModel model, int x, int y, out Vector2Int source)
        {
            for (int sy = y - 1; sy >= 0; sy--)
            {
                var c = new Vector2Int(x, sy);

                if (!model.IsCellActive(c))
                {
                    source = default;
                    return false;
                }

                var obj = model.GetGridObject(c);

                if (!obj) continue;

                if (obj.IsStationary)
                {
                    source = default;
                    return false;
                }

                source = c;
                return true;
            }

            source = default;
            return false;
        }

        private static bool DestinationBlockedByStationaryAbove(IGridModel model, Vector2Int empty)
        {
            for (int sy = empty.y - 1; sy >= 0; sy--)
            {
                var c = new Vector2Int(empty.x, sy);

                if (!model.IsCellActive(c)) return false;

                var obj = model.GetGridObject(c);

                if (!obj) continue;

                return obj.IsStationary;
            }

            return false;
        }

        private static bool TryCollectDiagonalSlide(IGridModel model, Vector2Int empty, int dirX, out SlideMoveRecord slide)
        {
            slide = default;

            var src = new Vector2Int(empty.x + dirX, empty.y - 1);

            if (!model.IsInRange(src)) return false;

            if (!model.IsCellActive(src)) return false;

            var item = model.GetGridObject(src);
            if (!item || item.IsStationary)
                return false;

            if (IsBlockerSideSource(model, src, dirX))
            {
                slide = new SlideMoveRecord(item, src, empty);
                return true;
            }

            if (IsBlockerShadowSandSource(model, src) && !HasEmptyBelowInSegment(model, src))
            {
                slide = new SlideMoveRecord(item, src, empty);
                return true;
            }

            return false;
        }

        private static bool IsBlockerSideSource(IGridModel model, Vector2Int src, int dirX)
        {
            var sideOfSource = new Vector2Int(src.x - dirX, src.y);

            if (!model.IsInRange(sideOfSource)) return false;

            var sideObj = model.GetGridObject(sideOfSource);
            if (!sideObj || !sideObj.IsStationary) return false;

            var slideSide = new Vector2Int(src.x + dirX, src.y);

            if (model.IsInRange(slideSide))
            {
                var slideSideObj = model.GetGridObject(slideSide);
                if (slideSideObj && slideSideObj.IsStationary) return false;
            }

            return true;
        }

        private static bool IsBlockerShadowSandSource(IGridModel model, Vector2Int src)
        {
            for (int sy = src.y - 1; sy >= 0; sy--)
            {
                var c = new Vector2Int(src.x, sy);

                if (!model.IsCellActive(c)) return false;

                var obj = model.GetGridObject(c);

                if (!obj) continue;

                return obj.IsStationary;
            }

            return false;
        }

        private static bool HasEmptyBelowInSegment(IGridModel model, Vector2Int src)
        {
            for (int sy = src.y + 1; sy < model.Height; sy++)
            {
                var c = new Vector2Int(src.x, sy);

                if (!model.IsCellActive(c)) return false;

                var obj = model.GetGridObject(c);

                if (obj && obj.IsStationary) return false;

                if (!obj) return true;
            }

            return false;
        }

        private void ApplyMove(IGridModel model, BaseGridObject item, Vector2Int from, Vector2Int to)
        {
            model.SetGridObject(from, null);
            model.SetGridObject(to, item);
            AddPath(item, to);
        }

        private void AddPath(BaseGridObject item, Vector2Int step)
        {
            if (!_pathByItem.TryGetValue(item, out var list))
            {
                list = new List<Vector2Int>(8);
                _pathByItem[item] = list;
            }

            if (list.Count == 0 || list[^1] != step)
                list.Add(step);
        }

        private UniTask PlayAnimations(GridStateContext context)
        {
            _animTasks.Clear();

            var view = context.View;
            var cellSize = view.GetCellSize();

            var durationMul = _settings.ShiftDurationMultiplier * (2f / Mathf.Max(0.0001f, cellSize));

            var byColumn = new Dictionary<int, List<(BaseGridObject item, List<Vector2Int> path, bool spawned, int finalY)>>(16);

            foreach (var kv in _pathByItem)
            {
                var item = kv.Key;
                var path = kv.Value;
                if (path == null || path.Count == 0) continue;

                var final = path[^1];
                if (!byColumn.TryGetValue(final.x, out var list))
                {
                    list = new List<(BaseGridObject, List<Vector2Int>, bool, int)>(16);
                    byColumn[final.x] = list;
                }

                list.Add((item, path, _spawned.Contains(item), final.y));
            }

            foreach (var col in byColumn)
            {
                var list = col.Value;

                list.Sort((a, b) =>
                {
                    if (a.spawned != b.spawned)
                        return a.spawned ? 1 : -1;

                    return b.finalY.CompareTo(a.finalY);
                });

                var nonSpawnCount = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].spawned)
                        nonSpawnCount++;
                }

                var nonSpawnWave = 0;
                var spawnWave = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    var (item, path, spawned, _) = list[i];

                    var world = new Vector3[path.Count + (spawned ? 1 : 0)];
                    var idx = 0;

                    if (spawned)
                        world[idx++] = item.transform.position;

                    for (int p = 0; p < path.Count; p++)
                        world[idx++] = view.GridToWorld(path[p]);

                    var wave = spawned ? (nonSpawnCount + spawnWave++) : nonSpawnWave++;
                    var delay = wave * _settings.ShiftDelayMultiplier;

                    var tween = item.ItemAnimation.ShiftPath(world, durationMul, delay);
                    _animTasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                }
            }

            return UniTask.WhenAll(_animTasks);
        }
    }
}
