using System;
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
    public sealed class SlideDownFillStrategy : IRefillStrategy
    {
        private readonly RefillSettingsSO _settings;

        private readonly Dictionary<BaseGridObject, List<Vector2Int>> _pathByItem = new(256);
        private readonly HashSet<BaseGridObject> _spawned = new(128);
        private readonly List<Vector2Int> _spawnCoords = new(64);

        private Task[] _animTasks = new Task[128];
        private Task _runningAnimations;
        
        private readonly List<SlideMoveRecord> _slides = new(128);
        private readonly HashSet<Vector2Int> _usedTargets = new();
        private readonly List<(BaseGridObject item, List<Vector2Int> path, bool spawned, int finalY, int finalX)> _allMoves = new(256);
        private readonly Dictionary<int, int> _columnCounter = new(16);
        private readonly Dictionary<int, int> _slideColumnCounter = new(16);

        public SlideDownFillStrategy(GameplayConfigContainer config)
        {
            _settings = config.RefillSettings;
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
                _slides.Clear();

                for (int y = height - 1; y >= 1; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var targetCoord = new Vector2Int(x, y);

                        if (!GridRefillCalcUtil.IsEmptyActiveCell(model, targetCoord)) continue;

                        if (GridRefillCalcUtil.CanFallVertically(model, x, y, out _)) continue;

                        if (!GridRefillCalcUtil.DestinationBlockedByStationaryAbove(model, targetCoord)) continue;

                        if (GridRefillCalcUtil.TryCollectDiagonalSlide(model, targetCoord, 1, out var right))
                            _slides.Add(right);

                        if (GridRefillCalcUtil.TryCollectDiagonalSlide(model, targetCoord, -1, out var left))
                            _slides.Add(left);
                    }
                }

                if (_slides.Count > 0)
                {
                    _usedTargets.Clear();

                    for (int i = 0; i < _slides.Count; i++)
                    {
                        var slide = _slides[i];

                        if (!_usedTargets.Add(slide.To)) continue;

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

            _runningAnimations = PlayAnimations(view);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations.AsUniTask();

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

            if (_spawnCoords.Count == 0) return false;
            
            var spawnY = view.GridToWorld(_spawnCoords[0]).y + cellSize;

            for (int i = 0; i < _spawnCoords.Count; i++)
            {
                var coord = _spawnCoords[i];

                var type = SmartSpawnDecider.Decide(model, coord, _settings.SpawnSettings, 0f);
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

        private Task PlayAnimations(IGridView view)
        {
            var cellSize = view.GetCellSize();

            _allMoves.Clear();

            foreach (var (item, path) in _pathByItem)
            {
                if (path == null || path.Count == 0) continue;
                var final = path[^1];
                _allMoves.Add((item, path, _spawned.Contains(item), final.y, final.x));
            }

            _allMoves.Sort((a, b) =>
            {
                if (a.spawned != b.spawned) return a.spawned ? 1 : -1;
                var y = b.finalY.CompareTo(a.finalY);
                return y != 0 ? y : a.finalX.CompareTo(b.finalX);
            });
            
            if (_animTasks.Length < _allMoves.Count)
                Array.Resize(ref _animTasks, _allMoves.Count);

            _columnCounter.Clear();
            _slideColumnCounter.Clear();
            var taskCount = 0;
            var slideCounter = 0;

            foreach (var (item, path, spawned, finalY, finalX) in _allMoves)
            {
                if (!item) continue;

                var startX = path[0].x;
                var isSlide = startX != finalX;
                if (isSlide)
                {
                    slideCounter++;
                }
                    
            }

            foreach (var (item, path, spawned, finalY, finalX) in _allMoves)
            {
                if (!item) continue;

                var length = path.Count + (spawned ? 1 : 0);
                var world = new Vector3[length];
                
                var idx = 0;
                if (spawned)
                    world[idx++] = item.transform.position;

                for (int p = 0; p < path.Count; p++)
                    world[idx++] = view.GridToWorld(path[p]);

                var startX = path[0].x;
                var isSlide = startX != finalX;

                int colIndex;
                if (isSlide)
                {
                    colIndex = _slideColumnCounter.GetValueOrDefault(finalX, 0);
                    _slideColumnCounter[finalX] = colIndex + 1;
                }
                else
                {
                    colIndex = _columnCounter.GetValueOrDefault(finalX, spawned ? slideCounter : 0);
                    _columnCounter[finalX] = colIndex + 1;
                }

                var minus = spawned ? 3 : 0;
                var colDelay = (colIndex - minus) * _settings.ShiftDelayMultiplier;

                var totalDuration = 0f;
                var current = item.transform.position;

                for (int w = 0; w < length; w++)
                {
                    var next = world[w];
                    var distCells = Mathf.Abs(current.y - next.y) / cellSize;
                    var segMul = .9f + distCells * _settings.ShiftDurationMultiplier;
                    totalDuration += segMul;
                    current = next;
                }
          
                var tween = item.ItemAnimation.ShiftPath(world, length, totalDuration, colDelay);
                _animTasks[taskCount++] = tween.AsyncWaitForCompletion();
               
            }

            return taskCount == 0 ? Task.CompletedTask : Task.WhenAll(_animTasks.AsSpan(0, taskCount).ToArray());
        }

    }
}