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
                
                if (!GridRefillCalc.HasAnyEmptyActiveCell(model)) break;

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

        private void MarkFallPlans(IGridModel model, int width, int height, List<MovePlan> plans, HashSet<Vector2Int> reservedSources)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var emptyCell = new Vector2Int(x, y);

                    if (!GridRefillCalc.IsEmptyActiveCell(model, emptyCell)) continue;

                    if (!GridRefillCalc.TryFindVerticalSource(model, emptyCell.x, emptyCell.y, out var sourceCell)) continue;

                    if (reservedSources.Contains(sourceCell)) continue;

                    var item = model.GetGridObject(sourceCell);
                    if (!item || item.IsStationary) continue;

                    plans.Add(new MovePlan(sourceCell, emptyCell, item, MoveKind.Fall));
                    reservedSources.Add(sourceCell);
                }
            }
        }

        private void MarkSlidePlans(IGridModel model, int width, int height, List<MovePlan> plans, HashSet<Vector2Int> reservedSources)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var emptyCell = new Vector2Int(x, y);

                    if (!GridRefillCalc.IsEmptyActiveCell(model, emptyCell)) continue;

                    if (!GridRefillCalc.TryGetBarrierYAbove(model, emptyCell.x, emptyCell.y, out var barrierY)) continue;

                    var topGapY = barrierY + 1;
                    if (topGapY >= height) continue;

                    if (emptyCell.y != topGapY) continue;

                    var topGapCell = new Vector2Int(emptyCell.x, topGapY);

                    if (!GridRefillCalc.IsEmptyActiveCell(model, topGapCell)) continue;

                    var barrierAtTop = GridRefillCalc.IsBarrierAtColumnTop(model, emptyCell.x, barrierY);

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

            if (!barrierAtTop && _spawnedInThisSim.Contains(item)) return false;

            if (item.IsStationary) return false;

            if (GridRefillCalc.CanFallStraightDown(model, sideCell)) return false;

            plans.Add(new MovePlan(sideCell, targetCell, item, MoveKind.Slide));
            reservedSources.Add(sideCell);
            return true;
        }

        private static void ApplyPlans(IGridModel model, List<MovePlan> plans, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            // Decide accepted plans: only first plan per destination survives
            var accepted = new bool[plans.Count];
            var usedDestinations = new HashSet<Vector2Int>(plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                var plan = plans[i];

                if (!usedDestinations.Add(plan.To))
                    continue;

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
                if (!current || current != plan.Item)
                    continue;

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
                if (!GridRefillCalc.TryGetSpawnCell(stateContext.Model, x, height, out var spawnCell)) continue;
                if (SpawnTopOpenCells(stateContext, view, x, height, cellSize, spawnCell, pathByItem)) spawnedAny = true;
            }

            return spawnedAny;
        }

        private bool SpawnTopOpenCells(GridStateContext stateContext, IGridView view, int x, int height, float cellSize, Vector2Int spawnCell, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem)
        {
            var model = stateContext.Model;

            _spawnCells.Clear();

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
            var spawnY = spawnWorld.y + cellSize;

            for (int i = 0; i < _spawnCells.Count; i++)
            {
                var cell = _spawnCells[i];
                var item = stateContext.Factory.GetRandomItem();

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var targetWorld = view.GridToWorld(cell);

                item.SetPosition(new Vector3(targetWorld.x, spawnY, targetWorld.z));

                model.SetGridObject(cell, item);

                _spawnedInThisSim.Add(item);

                AddPathStep(pathByItem, item, cell);
            }

            return true;
        }

        private static void BuildFinalCellRecords(IGridModel model, int width, int height, Dictionary<BaseGridObject, List<Vector2Int>> pathByItem, ItemMoveRecord[,] records)
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

            if (list.Count > 0 && list[^1] == step) return;

            list.Add(step);
        }
    }
}