using System.Collections.Generic;
using Core.Config;
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
        private const float SpawnYOffset = 1.25f;

        private RefillSettings _refillSettings;

        private readonly List<Vector2Int> _spawnCells = new();
        private readonly HashSet<BaseGridObject> _spawnedInThisSim = new();

        public void SetRefillSettings(RefillSettings refillSettings)
        {
            _refillSettings = refillSettings;
        }

        public bool CanRefill(IGridModel model) => GridRefillCalc.HasStationaryAndBlocking(model);

        public async UniTask Execute(GridStateContext context, List<UniTask> tasks)
        {
            await UniTask.WaitForSeconds(_refillSettings.RefillStartDelay);

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            var pathByItem = new Dictionary<BaseGridObject, List<Vector2Int>>(width * height);
            var records = new ItemMoveRecord[width, height];

            _spawnedInThisSim.Clear();

            var outerSafety = width * height * 12;

            while (outerSafety-- > 0)
            {
                var movedAny = MarkAndApplyMoves(context, width, height, pathByItem);
                var spawnedAny = SpawnRefill(context, view, width, height, cellSize, pathByItem);

                if (!movedAny && !spawnedAny) break;
            }

            BuildFinalCellRecords(model, width, height, pathByItem, records);
            PlayMoveAnimation(model, view, width, height, records, tasks);

            await UniTask.CompletedTask;
        }

        private bool MarkAndApplyMoves(GridStateContext stateContext, int width, int height, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var model = stateContext.Model;

            var movedAnyOverall = false;

            var safety = width * height * 12;

            while (safety-- > 0)
            {
                var plans = new List<MovePlan>(64);

                // Mark: fall moves (bottom-up, per empty cell)
                MarkFallPlans(model, width, height, plans);

                // Mark: slide moves (top-gap under barrier)
                MarkSlidePlans(model, width, height, plans);

                if (plans.Count == 0) break;

                // Apply: execute all marked moves in a safe way (destinations unique)
                ApplyPlans(model, plans, pathByItem);

                movedAnyOverall = true;
            }

            return movedAnyOverall;
        }

        private void MarkFallPlans(IGridModel model, int width, int height, List<MovePlan> plans)
        {
            // Prevent using the same source twice in this pass
            var reservedSources = new HashSet<Vector2Int>();

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var emptyCell = new Vector2Int(x, y);

                    if (!IsEmptyActiveCell(model, emptyCell)) continue;

                    if (!GridRefillCalc.TryFindVerticalSource(model, emptyCell.x, emptyCell.y, out var sourceCell)) continue;

                    if (reservedSources.Contains(sourceCell)) continue;

                    var item = model.GetGridObject(sourceCell);
                    if (!item || item.IsStationary) continue;

                    plans.Add(new MovePlan(sourceCell, emptyCell, item, MoveKind.Fall));
                    reservedSources.Add(sourceCell);
                }
            }
        }

        private void MarkSlidePlans(IGridModel model, int width, int height, List<MovePlan> plans)
        {
            // Prevent using the same source twice in this pass
            var reservedSources = new HashSet<Vector2Int>();

            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var emptyCell = new Vector2Int(x, y);

                    if (!IsEmptyActiveCell(model, emptyCell)) continue;

                    if (!GridRefillCalc.TryGetBarrierYAbove(model, emptyCell.x, emptyCell.y, out var barrierY)) continue;

                    var topGapY = barrierY + 1;
                    if (topGapY >= height) continue;

                    // Only the top-most gap under the barrier can be filled by sliding
                    if (emptyCell.y != topGapY) continue;

                    var topGapCell = new Vector2Int(emptyCell.x, topGapY);

                    if (!IsEmptyActiveCell(model, topGapCell)) continue;

                    var barrierAtTop = IsBarrierAtColumnTop(model, emptyCell.x, barrierY);

                    // Try right then left (keep priority)
                    if (TryMarkSlideFromSide(model, topGapCell, emptyCell.x + 1, barrierY, barrierAtTop, reservedSources, plans)) continue;

                    TryMarkSlideFromSide(model, topGapCell, emptyCell.x - 1, barrierY, barrierAtTop, reservedSources, plans);
                }
            }
        }

        private bool TryMarkSlideFromSide(IGridModel model, Vector2Int targetCell, int sideX, int sourceY, bool barrierAtTop, HashSet<Vector2Int> reservedSources, List<MovePlan> plans)
        {
            if (sideX < 0 || sideX >= model.Width) return false;

            var sideCell = new Vector2Int(sideX, sourceY);
            if (reservedSources.Contains(sideCell)) return false;
            if (!model.IsCellActive(sideCell)) return false;
            var item = model.GetGridObject(sideCell);
            if (!item) return false;

            // Prevent sliding freshly spawned items unless the barrier is at the column top
            if (!barrierAtTop && _spawnedInThisSim.Contains(item)) return false;

            if (item.IsStationary) return false;

            // If the side item can fall straight down, do not use it as a donor for sliding.
            if (GridRefillCalc.CanFallStraightDown(model, sideCell)) return false;

            plans.Add(new MovePlan(sideCell, targetCell, item, MoveKind.Slide));
            reservedSources.Add(sideCell);
            return true;
        }

        private static void ApplyPlans(IGridModel model, List<MovePlan> plans, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            // Destinations should be unique by construction; still guard to avoid overwrites.
            var usedDestinations = new HashSet<Vector2Int>(plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                if (!usedDestinations.Add(plans[i].To))
                    continue;
            }

            // First clear all sources, then set destinations (prevents accidental donor reads later in this pass)
            for (int i = 0; i < plans.Count; i++)
            {
                var plan = plans[i];

                var current = model.GetGridObject(plan.From);
                if (!current || current != plan.Item)
                    continue;

                model.SetGridObject(plan.From, null);
            }

            for (int i = 0; i < plans.Count; i++)
            {
                var plan = plans[i];

                // Record step into destination
                AddPathStep(pathByItem, plan.Item, plan.To);

                model.SetGridObject(plan.To, plan.Item);
            }
        }

        private bool IsEmptyActiveCell(IGridModel model, Vector2Int cell)
        {
            if (!model.IsCellActive(cell)) return false;
            return !model.GetGridObject(cell);
        }

        private bool IsBarrierAtColumnTop(IGridModel model, int x, int barrierY)
        {
            // If there is any active cell above the barrier, then the barrier is not at the top.
            for (int y = barrierY - 1; y >= 0; y--)
            {
                var cell = new Vector2Int(x, y);
                if (!model.IsCellActive(cell)) continue;

                return false;
            }

            return true;
        }

        private bool SpawnRefill(GridStateContext stateContext, IGridView view, int width, int height, float cellSize, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var spawnedAny = false;

            for (int x = 0; x < width; x++)
            {
                if (!GridRefillCalc.TryGetSpawnCell(stateContext.Model, x, height, out var spawnCell)) continue;
                if (SpawnTopOpenCells(stateContext, view, x, height, cellSize, spawnCell, pathByItem)) spawnedAny = true;
            }

            return spawnedAny;
        }

        private bool SpawnTopOpenCells(GridStateContext stateContext, IGridView view, int x, int height, float cellSize, Vector2Int spawnCell, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var model = stateContext.Model;

            _spawnCells.Clear();

            // Collect consecutive empty active cells from the top until we hit an existing object or a stationary barrier
            for (int y = 0; y < height; y++)
            {
                var cell = new Vector2Int(x, y);
                if (!model.IsCellActive(cell)) continue;
                var existing = model.GetGridObject(cell);
                if (existing && existing.IsStationary) break;
                if (existing) break;
                _spawnCells.Add(cell);
            }

            if (_spawnCells.Count == 0) return false;

            var spawnWorld = view.GridToWorld(spawnCell);
            var spawnY = spawnWorld.y + cellSize * SpawnYOffset;

            for (int i = 0; i < _spawnCells.Count; i++)
            {
                var cell = _spawnCells[i];
                var item = stateContext.Factory.GetRandomItem();

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var targetWorld = view.GridToWorld(cell);

                // Place spawned item above the grid visually
                item.SetPosition(new Vector3(targetWorld.x, spawnY, targetWorld.z));

                model.SetGridObject(cell, item);

                _spawnedInThisSim.Add(item);

                // Record the first step into the grid
                AddPathStep(pathByItem, item, cell);
            }

            return true;
        }

        private void BuildFinalCellRecords(IGridModel model, int width, int height, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem, ItemMoveRecord[,] records)
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

                    records[x, y] = new ItemMoveRecord
                    {
                        Path = path.ToArray()
                    };
                }
            }
        }

        private void PlayMoveAnimation(IGridModel model, IGridView view, int width, int height, ItemMoveRecord[,] records, List<UniTask> tasks)
        {
            for (int x = 0; x < width; x++)
            {
                var waveIndex = 0;

                for (int y = height - 1; y >= 0; y--)
                {
                    var record = records[x, y];
                    if (record.Path == null || record.Path.Length == 0) continue;

                    var cell = new Vector2Int(x, y);
                    var item = model.GetGridObject(cell);
                    if (!item) continue;

                    var delay = waveIndex * _refillSettings.ShiftDelayMultiplier;
                    waveIndex++;

                    if (HasHorizontalStep(record.Path)) 
                        delay = 0f;

                    var worldPoints = new Vector3[record.Path.Length];

                    for (int i = 0; i < record.Path.Length; i++)
                        worldPoints[i] = view.GridToWorld(record.Path[i]);

                    var tween = item.ItemAnimation.ShiftPath(worldPoints, _refillSettings.ShiftDurationMultiplier, delay);
                    
                    if (tween == null) continue;

                    tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                }
            }
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

            // Avoid duplicating the same cell consecutively
            if (list.Count > 0 && list[^1] == step)
                return;

            list.Add(step);
        }
    }
}