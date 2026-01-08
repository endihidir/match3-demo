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
        private UniTask _running;

        private readonly List<Vector2Int> _spawnCoords = new(64);

        public SlideDownFillStrategy(GameplayConfigContainer config)
        {
            _settings = config.ItemConfigContainer.RefillSettingsSo;
        }

        public bool CanRefill(IGridModel model) => GridRefillCalcUtil.HasStationaryAndBlocking(model);

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

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, dst)) continue;

                        if (!GridRefillCalcUtil.CanFallVertically(model, x, y, out var src)) continue;

                        var item = model.GetGridObject(src);
                        
                        if (!item || item.IsStationary) continue;

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

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, empty)) continue;

                        if (GridRefillCalcUtil.CanFallVertically(model, x, y, out _)) continue;

                        if (!GridRefillCalcUtil.DestinationBlockedByStationaryAbove(model, empty)) continue;

                        if (GridRefillCalcUtil.TryCollectDiagonalSlide(model, empty, 1, out var right))
                            slides.Add(right);

                        if (GridRefillCalcUtil.TryCollectDiagonalSlide(model, empty, -1, out var left))
                            slides.Add(left);
                    }
                }

                if (slides.Count > 0)
                {
                    var usedTargets = new HashSet<Vector2Int>();

                    for (int i = 0; i < slides.Count; i++)
                    {
                        var slide = slides[i];

                        if (!usedTargets.Add(slide.To)) continue;

                        ApplyMove(model, slide.Item, slide.From, slide.To);
                        movedAny = true;
                    }
                }

                // 3) Spawn into top-open segments (MIN PATCH: single spawnY per column pass)
                for (int x = 0; x < width; x++)
                {
                    if (SpawnTopOpenSegmentsInColumn(context, view, x, height))
                        movedAny = true;
                }
            }

            _running = PlayAnimations(context);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _running;

        private bool SpawnTopOpenSegmentsInColumn(GridStateContext context, IGridView view, int x, int height)
        {
            var model = context.Model;
            var cellSize = view.GetCellSize();

            _spawnCoords.Clear();

            var segmentBlocked = false;

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

                _spawnCoords.Add(c);
            }

            if (_spawnCoords.Count == 0)
                return false;

            // MIN PATCH: all spawns start from a single Y (FallDown-like), not per-target-cell Y.
            var spawnY = view.GridToWorld(_spawnCoords[0]).y + cellSize;

            for (int i = 0; i < _spawnCoords.Count; i++)
            {
                var coord = _spawnCoords[i];

                var type = SmartSpawnDecider.Decide(model, coord, _settings.SpawnSettings);
                var item = context.Factory.GetRegularItem(type);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var w = view.GridToWorld(coord);
                item.SetPosition(new Vector3(w.x, spawnY, w.z));

                model.SetGridObject(coord, item);
                _spawned.Add(item);

                AddPath(item, coord);
            }

            return true;
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

            var all = new List<(BaseGridObject item, List<Vector2Int> path, bool spawned, int finalY, int finalX)>(_pathByItem.Count);

            foreach (var kv in _pathByItem)
            {
                var item = kv.Key;
                var path = kv.Value;
                if (path == null || path.Count == 0) continue;

                var final = path[^1];
                all.Add((item, path, _spawned.Contains(item), final.y, final.x));
            }

            all.Sort((a, b) =>
            {
                if (a.spawned != b.spawned) return a.spawned ? 1 : -1;
                var y = b.finalY.CompareTo(a.finalY);
                if (y != 0) return y;
                return a.finalX.CompareTo(b.finalX);
            });

            var columnCounter = new Dictionary<int, int>(16);
            var slideColumnCounter = new Dictionary<int, int>(16);

            foreach (var (item, path, spawned, finalY, finalX) in all)
            {
                if (!item) continue;

                var world = new Vector3[path.Count + (spawned ? 1 : 0)];
                var idx = 0;

                if (spawned)
                    world[idx++] = item.transform.position;

                for (int p = 0; p < path.Count; p++)
                    world[idx++] = view.GridToWorld(path[p]);

                var startX = path[0].x;
                var isSlide = startX != finalX;

                var delayMultiplier = isSlide ? (_settings.ShiftDelayMultiplier / 5f) : _settings.ShiftDelayMultiplier;

                var colIndex = isSlide
                    ? slideColumnCounter.GetValueOrDefault(finalX, 0)
                    : columnCounter.GetValueOrDefault(finalX, 0);

                var colDelay = colIndex * delayMultiplier;

                if (isSlide)
                    slideColumnCounter[finalX] = colIndex + 1;
                else
                    columnCounter[finalX] = colIndex + 1;

                var totalDuration = 0f;
                var current = item.transform.position;

                for (int w = 0; w < world.Length; w++)
                {
                    var next = world[w];
                    var distCells = Mathf.Abs(current.y - next.y) / cellSize;
                    var segMul = 0.5f + distCells * _settings.ShiftDurationMultiplier;
                    totalDuration += segMul;
                    current = next;
                }

                var tween = item.ItemAnimation.ShiftPath(world, totalDuration, colDelay);
                _animTasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
            }

            return UniTask.WhenAll(_animTasks);
        }


    }
}