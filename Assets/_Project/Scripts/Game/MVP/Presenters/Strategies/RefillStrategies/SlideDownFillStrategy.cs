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
    public sealed class SlideDownRefillStrategy : IRefillStrategy
    {
        private readonly RefillSettingsSO _refillSettingsSo;
        private readonly List<Vector2Int> _spawnCoords = new();
        private readonly List<UniTask> _animTasks = new(128);
        private readonly HashSet<BaseGridObject> _spawnedInThisSim = new();
        private UniTask _runningAnimations;

        public SlideDownRefillStrategy(GameplayConfigContainer configContainer)
        {
            _refillSettingsSo = configContainer.ItemConfigContainer.RefillSettingsSo;
        }
        
        public bool CanRefill(IGridModel model) => GridRefillCalcUtil.HasStationaryAndBlocking(model);

        public IRefillStrategy Execute(GridStateContext context)
        {
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            var pathByItem = new Dictionary<BaseGridObject, List<Vector2Int>>(width * height);
            var records = new SlideMoveRecord[width, height];

            _spawnedInThisSim.Clear();

            var outerSafety = width * height * 12;

            while (outerSafety-- > 0)
            {
                var movedAny = MarkAndApplyMoves(context, width, height, pathByItem);
                var spawnedAny = SpawnRefill(context, view, width, height, cellSize, pathByItem);

                if (!movedAny && !spawnedAny) break;
            }

            BuildFinalCellRecords(model, width, height, pathByItem, records);
            _runningAnimations = PlayMoveAnimation(model, view, width, height, records);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        private bool MarkAndApplyMoves(GridStateContext stateContext, int width, int height, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var model = stateContext.Model;

            var movedAnyOverall = false;

            var safety = width * height * 12;

            while (safety-- > 0)
            {
                var plans = new List<SlideMovePlan>(64);

                // IMPORTANT: prevent using the same source twice across FALL+SLIDE in the same pass
                var reservedSources = new HashSet<Vector2Int>();

                MarkFallPlans(model, width, height, plans, reservedSources);
                MarkSlidePlans(model, width, height, plans, reservedSources);

                if (plans.Count == 0) break;

                ApplyPlans(model, plans, pathByItem);

                movedAnyOverall = true;
            }

            return movedAnyOverall;
        }

        private void MarkFallPlans(IGridModel model, int width, int height, List<SlideMovePlan> plans, HashSet<Vector2Int> reservedSourceCoords)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var coord = new Vector2Int(x, y);

                    if (!GridRefillCalcUtil.IsEmptyActiveCell(model, coord)) continue;

                    if (!GridRefillCalcUtil.TryFindVerticalSource(model, coord.x, coord.y, out var sourceCoord)) continue;

                    if (reservedSourceCoords.Contains(sourceCoord)) continue;

                    var item = model.GetGridObject(sourceCoord);
                    if (!item || item.IsStationary) continue;

                    plans.Add(new SlideMovePlan(sourceCoord, coord, item, false));
                    reservedSourceCoords.Add(sourceCoord);
                }
            }
        }

        private void MarkSlidePlans(IGridModel model, int width, int height, List<SlideMovePlan> plans, HashSet<Vector2Int> reservedSources)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var coord = new Vector2Int(x, y);

                    if (!GridRefillCalcUtil.IsEmptyActiveCell(model, coord)) continue;

                    if (!GridRefillCalcUtil.TryGetBarrierYAbove(model, coord.x, coord.y, out var barrierY)) continue;

                    var topGapY = barrierY + 1;
                    if (topGapY >= height) continue;

                    if (coord.y != topGapY) continue;

                    var topGapCell = new Vector2Int(coord.x, topGapY);

                    if (!GridRefillCalcUtil.IsEmptyActiveCell(model, topGapCell)) continue;

                    var barrierAtTop = GridRefillCalcUtil.IsBarrierAtColumnTop(model, coord.x, barrierY);

                    if (TryMarkSlideFromSide(model, topGapCell, coord.x + 1, barrierY, barrierAtTop, reservedSources, plans)) continue;

                    TryMarkSlideFromSide(model, topGapCell, coord.x - 1, barrierY, barrierAtTop, reservedSources, plans);
                }
            }
        }

        private bool TryMarkSlideFromSide(IGridModel model, Vector2Int targetCell, int sideX, int sourceY, bool barrierAtTop, HashSet<Vector2Int> reservedSources, List<SlideMovePlan> plans)
        {
            if (sideX < 0 || sideX >= model.Width) return false;

            var sideCellCoord = new Vector2Int(sideX, sourceY);

            if (reservedSources.Contains(sideCellCoord)) return false;
            if (!model.IsCellActive(sideCellCoord)) return false;

            var item = model.GetGridObject(sideCellCoord);
            if (!item) return false;

            if (!barrierAtTop && _spawnedInThisSim.Contains(item)) return false;

            if (item.IsStationary) return false;

            if (GridRefillCalcUtil.CanFallStraightDown(model, sideCellCoord)) return false;

            plans.Add(new SlideMovePlan(sideCellCoord, targetCell, item, true));
            reservedSources.Add(sideCellCoord);
            return true;
        }

        private static void ApplyPlans(IGridModel model, List<SlideMovePlan> plans, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            // Decide accepted plans: only first plan per destination survives
            var accepted = new bool[plans.Count];
            var usedDestinations = new HashSet<Vector2Int>(plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                var plan = plans[i];

                if (!usedDestinations.Add(plan.To)) continue;

                accepted[i] = true;

                // Ensure the first path step contains the source cell
                if (!pathByItem.TryGetValue(plan.Item, out var list) || list.Count == 0)
                    AddPathStep(pathByItem, plan.Item, plan.From);
            }

            // Clear sources (only accepted)
            for (int i = 0; i < plans.Count; i++)
            {
                if (!accepted[i]) continue;

                var plan = plans[i];

                var current = model.GetGridObject(plan.From);
                if (!current || current != plan.Item) continue;

                model.SetGridObject(plan.From, null);
            }

            // Set destinations + record steps (only accepted)
            for (int i = 0; i < plans.Count; i++)
            {
                if (!accepted[i]) continue;

                var plan = plans[i];

                AddPathStep(pathByItem, plan.Item, plan.To);
                model.SetGridObject(plan.To, plan.Item);
            }
        }

        private bool SpawnRefill(GridStateContext stateContext, IGridView view, int width, int height, float cellSize, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var spawnedAny = false;

            for (int x = 0; x < width; x++)
            {
                if (!GridRefillCalcUtil.TryGetSpawnCellCoord(stateContext.Model, x, height, out var cellCoord)) continue;
                
                if (SpawnTopOpenCells(stateContext, view, x, height, cellSize, cellCoord, pathByItem))
                {
                    spawnedAny = true;
                }
            }

            return spawnedAny;
        }

        private bool SpawnTopOpenCells(GridStateContext stateContext, IGridView view, int x, int height, float cellSize, Vector2Int cellCoord, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var model = stateContext.Model;

            _spawnCoords.Clear();

            for (int y = 0; y < height; y++)
            {
                var coord = new Vector2Int(x, y);
                if (!model.IsCellActive(coord)) continue;

                var existing = model.GetGridObject(coord);
                if (existing && existing.IsStationary) break;
                if (existing) break;

                _spawnCoords.Add(coord);
            }

            if (_spawnCoords.Count == 0) return false;

            var spawnWorld = view.GridToWorld(cellCoord);
            var spawnY = spawnWorld.y + cellSize;

            for (int i = 0; i < _spawnCoords.Count; i++)
            {
                var coord = _spawnCoords[i];
                var itemType = SmartSpawnDecider.Decide(model, coord, _refillSettingsSo.SpawnSettings);
                var item = stateContext.Factory.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var targetWorld = view.GridToWorld(coord);

                item.SetPosition(new Vector3(targetWorld.x, spawnY, targetWorld.z));

                model.SetGridObject(coord, item);

                _spawnedInThisSim.Add(item);

                AddPathStep(pathByItem, item, coord);
            }

            return true;
        }

        private static void BuildFinalCellRecords(IGridModel model, int width, int height, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem, SlideMoveRecord[,] records)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new Vector2Int(x, y);

                    if (!model.IsCellActive(cell)) continue;

                    var item = model.GetGridObject(cell);
                    
                    if (!item) continue;

                    if (!pathByItem.TryGetValue(item, out var path) || path.Count == 0) continue;

                    records[x, y] = new SlideMoveRecord
                    {
                        ItemCoordPath = path.ToArray()
                    };
                }
            }
        }

        private async UniTask PlayMoveAnimation(IGridModel model, IGridView view, int width, int height, SlideMoveRecord[,] records)
        {
            _animTasks.Clear();
            
            for (int x = 0; x < width; x++)
            {
                // 1) Count non-spawn moves in this column so that spawns start AFTER them.
                var nonSpawnMoveCount = 0;

                for (int y = height - 1; y >= 0; y--)
                {
                    var record = records[x, y];
                    if (record.ItemCoordPath == null || record.ItemCoordPath.Length == 0) continue;

                    var cell = new Vector2Int(x, y);
                    
                    var item = model.GetGridObject(cell);
                    
                    if (!item) continue;

                    if (_spawnedInThisSim.Contains(item)) continue;

                    nonSpawnMoveCount++;
                }

                // 2) Animate: non-spawn first (wave 0..), spawns after (wave nonSpawnMoveCount..)
                var nonSpawnWaveIndex = 0;
                var spawnWaveIndex = 0;

                for (int y = height - 1; y >= 0; y--)
                {
                    var record = records[x, y];
                    if (record.ItemCoordPath == null || record.ItemCoordPath.Length == 0) continue;

                    var cell = new Vector2Int(x, y);
                    var item = model.GetGridObject(cell);
                    if (!item) continue;

                    var isSpawned = _spawnedInThisSim.Contains(item);

                    var waveIndex = isSpawned ? nonSpawnMoveCount + spawnWaveIndex++ : nonSpawnWaveIndex++;

                    var delay = (waveIndex * _refillSettingsSo.ShiftDelayMultiplier);

                    if (HasHorizontalStep(record.ItemCoordPath))
                        delay = 0f;

                    // Build world points.
                    // For spawned items, prepend current position (spawnY) so it actually "falls" instead of instantly settling.
                    Vector3[] worldPoints;

                    if (isSpawned)
                    {
                        worldPoints = new Vector3[record.ItemCoordPath.Length + 1];
                        worldPoints[0] = item.transform.position;

                        for (int i = 0; i < record.ItemCoordPath.Length; i++)
                            worldPoints[i + 1] = view.GridToWorld(record.ItemCoordPath[i]);
                    }
                    else
                    {
                        worldPoints = new Vector3[record.ItemCoordPath.Length];

                        for (int i = 0; i < record.ItemCoordPath.Length; i++)
                            worldPoints[i] = view.GridToWorld(record.ItemCoordPath[i]);
                    }

                    var tween = item.ItemAnimation.ShiftPath(worldPoints, _refillSettingsSo.ShiftDurationMultiplier, delay);

                    if (tween == null) continue;

                    _animTasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                }
            }
            
            await UniTask.WhenAll(_animTasks);
        }

        private static bool HasHorizontalStep(Vector2Int[] path)
        {
            for (int i = 1; i < path.Length; i++)
            {
                if (path[i].x != path[i - 1].x)
                    return true;
            }

            return false;
        }

        private static void AddPathStep(Dictionary<BaseGridObject, List<Vector2Int>> pathByItem, BaseGridObject item, Vector2Int step)
        {
            if (!pathByItem.TryGetValue(item, out var list))
            {
                list = new List<Vector2Int>(8);
                pathByItem.Add(item, list);
            }

            if (list.Count > 0 && list[^1] == step) return;

            list.Add(step);
        }
    }
}