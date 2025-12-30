using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideDownRefillStrategy : IRefillStrategy
    {
        private const float SpawnYOffset = 1.25f;
       
        private float _startDelay;
        private float _shiftDurationMultiplier;
        private float _shiftDelayMultiplier;

        private readonly List<Vector2Int> _spawnCells = new();
        private readonly HashSet<BaseGridObject> _spawnedInThisSim = new();

        private struct MoveInfo
        {
            public BaseGridObject grid;
            public Vector3 StartWorld;
            public Vector2Int TargetCell;
            public bool HasSlide;
            public Vector2Int SlideTargetCell;
        }
        
        public void SetRefillSettings(RefillSettings refillSettings)
        {
            _shiftDurationMultiplier = refillSettings.ShiftDurationMultiplier;
            _shiftDelayMultiplier = refillSettings.ShiftDelayMultiplier;
        }
        
        public bool CanRefill(IGridModel model) => model.HasStationaryAndBlocking();

        public async UniTask Execute(GridStateContext context, List<UniTask> tasks)
        {
            await UniTask.WaitForSeconds(_startDelay);
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            var movedSet = new HashSet<BaseGridObject>(width * height);
            var startWorldByItem = new Dictionary<BaseGridObject, Vector3>(width * height);
            var slideStepByItem = new Dictionary<BaseGridObject, Vector2Int>(width * height);

            SimulateGravityAndSlides(context, width, height, cellSize, movedSet, startWorldByItem, slideStepByItem);
            SpawnRefill(context, view, width, height, cellSize, movedSet, startWorldByItem);

            var finalCellByItem = BuildFinalCellMap(model, width, height, movedSet);
            PlayMoveAnimations(view, width, cellSize, movedSet, startWorldByItem, finalCellByItem, slideStepByItem, tasks);

            await UniTask.CompletedTask;
        }

        private void SimulateGravityAndSlides(GridStateContext stateContext, int width, int height, float cellSize, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem, Dictionary<BaseGridObject, Vector2Int> slideStepByItem)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            _spawnedInThisSim.Clear();

            var anyMoved = true;
            var safety = width * height * 12;

            while (anyMoved && safety-- > 0)
            {
                anyMoved = false;

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var emptyCell = new Vector2Int(x, y);

                        if (!IsEmptyActiveCell(model, emptyCell)) continue;

                        if (TryFallIntoCell(model, emptyCell, movedSet, startWorldByItem))
                            anyMoved = true;
                    }
                }

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var emptyCell = new Vector2Int(x, y);

                        if (!IsEmptyActiveCell(model, emptyCell)) continue;

                        if (TrySlideIntoTopGap(model, emptyCell, height, movedSet, startWorldByItem, slideStepByItem))
                            anyMoved = true;
                    }
                }

                var spawnedAny = false;

                for (int x = 0; x < width; x++)
                {
                    if (!TryGetSpawnYForColumn(model, view, x, height, cellSize, out var spawnY))
                        continue;

                    var before = movedSet.Count;

                    SpawnTopOpenCells(stateContext, view, x, height, cellSize, spawnY, movedSet, startWorldByItem);

                    if (movedSet.Count != before)
                        spawnedAny = true;
                }

                if (spawnedAny)
                    anyMoved = true;
            }
        }

        private bool IsEmptyActiveCell(IGridModel model, Vector2Int cell)
        {
            if (!model.IsCellActive(cell)) return false;
            return !model.GetGridObject(cell);
        }

        private bool TryFallIntoCell(IGridModel model, Vector2Int emptyCell, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem)
        {
            if (!model.TryFindVerticalSource(emptyCell.x, emptyCell.y, out var sourceCell)) return false;

            var item = model.GetGridObject(sourceCell);
            if (!item || item.IsStationary) return false;

            TrackMovedItem(item, movedSet, startWorldByItem);

            model.SetGridObject(emptyCell, item);
            model.SetGridObject(sourceCell, null);

            return true;
        }

        private bool TrySlideIntoTopGap(IGridModel model, Vector2Int emptyCell, int height, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem, Dictionary<BaseGridObject, Vector2Int> slideStepByItem)
        {
            if (!model.TryGetBarrierYAbove(emptyCell.x, emptyCell.y, out var barrierY)) return false;

            var topGapY = barrierY + 1;
            if (topGapY >= height) return false;
            if (emptyCell.y != topGapY) return false;
            var topGapCell = new Vector2Int(emptyCell.x, topGapY);

            if (!IsEmptyActiveCell(model, topGapCell)) return false;

            var barrierAtTop = IsBarrierAtColumnTop(model, emptyCell.x, barrierY);

            return TrySlideFromSideIntoCell(model, topGapCell, emptyCell.x + 1, barrierY, barrierAtTop, movedSet, startWorldByItem, slideStepByItem) ||
                   TrySlideFromSideIntoCell(model, topGapCell, emptyCell.x - 1, barrierY, barrierAtTop, movedSet, startWorldByItem, slideStepByItem);
        }

        private bool IsBarrierAtColumnTop(IGridModel model, int x, int barrierY)
        {
            for (int y = barrierY - 1; y >= 0; y--)
            {
                var cell = new Vector2Int(x, y);
                if (!model.IsCellActive(cell)) continue;
                return false;
            }

            return true;
        }

        private bool TrySlideFromSideIntoCell(IGridModel model, Vector2Int targetCell, int sideX, int sourceY, bool barrierAtTop, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem, Dictionary<BaseGridObject, Vector2Int> slideStepByItem)
        {
            if (sideX < 0 || sideX >= model.Width) return false;

            var sideCell = new Vector2Int(sideX, sourceY);
            if (!model.IsCellActive(sideCell)) return false;

            var item = model.GetGridObject(sideCell);
            if (!item) return false;

            if (!barrierAtTop && _spawnedInThisSim.Contains(item))
                return false;

            if (item.IsStationary) return false;
            if (model.CanFallStraightDown(sideCell)) return false;

            TrackMovedItem(item, movedSet, startWorldByItem);
            model.SetGridObject(targetCell, item);
            model.SetGridObject(sideCell, null);
            slideStepByItem[item] = targetCell;

            return true;
        }

        private void TrackMovedItem(BaseGridObject grid, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem)
        {
            movedSet.Add(grid);

            if (!startWorldByItem.ContainsKey(grid))
                startWorldByItem.Add(grid, grid.transform.position);
        }

        private void SpawnRefill(GridStateContext stateContext, IGridView view, int width, int height, float cellSize, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem)
        {
            for (int x = 0; x < width; x++)
            {
                if (!TryGetSpawnYForColumn(stateContext.Model, view, x, height, cellSize, out var spawnY))
                    continue;

                SpawnTopOpenCells(stateContext, view, x, height, cellSize, spawnY, movedSet, startWorldByItem);
            }
        }

        private bool TryGetSpawnYForColumn(IGridModel model, IGridView view, int x, int height, float cellSize, out float spawnY)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = new Vector2Int(x, y);

                if (!model.IsCellActive(cell)) continue;

                spawnY = view.GridToWorld(cell).y + cellSize * SpawnYOffset;
                return true;
            }

            spawnY = 0f;
            return false;
        }

        private void SpawnTopOpenCells(GridStateContext stateContext, IGridView view, int x, int height, float cellSize, float spawnY, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem)
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
                TrackMovedItem(item, movedSet, startWorldByItem);
            }
        }

        private Dictionary<BaseGridObject, Vector2Int> BuildFinalCellMap(IGridModel model, int width, int height, HashSet<BaseGridObject> movedSet)
        {
            var map = new Dictionary<BaseGridObject, Vector2Int>(movedSet.Count);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!model.IsCellActive(cell)) continue;
                    var item = model.GetGridObject(cell);
                    if (!item) continue;
                    if (!movedSet.Contains(item)) continue;
                    map.TryAdd(item, cell);
                }
            }

            return map;
        }

        private void PlayMoveAnimations(IGridView view, int width, float cellSize, HashSet<BaseGridObject> movedSet, Dictionary<BaseGridObject, Vector3> startWorldByItem, Dictionary<BaseGridObject, Vector2Int> finalCellByItem, Dictionary<BaseGridObject, Vector2Int> slideStepByItem, List<UniTask> tasks)
        {
            var movesByColumn = new List<MoveInfo>[width];

            for (int x = 0; x < width; x++)
                movesByColumn[x] = new List<MoveInfo>();

            foreach (var item in movedSet)
            {
                if (!item) continue;
                if (!finalCellByItem.TryGetValue(item, out var finalCell)) continue;

                if (!startWorldByItem.TryGetValue(item, out var startWorld))
                    startWorld = item.transform.position;

                var hasSlide = slideStepByItem.TryGetValue(item, out var slideCell);

                movesByColumn[finalCell.x].Add(new MoveInfo
                {
                    grid = item,
                    StartWorld = startWorld,
                    TargetCell = finalCell,
                    HasSlide = hasSlide,
                    SlideTargetCell = slideCell
                });
            }

            var swipeDelay = 0f;

            for (int x = 0; x < width; x++)
            {
                movesByColumn[x].Sort((a, b) => b.TargetCell.y.CompareTo(a.TargetCell.y));

                var waveIndex = 0;

                for (int i = 0; i < movesByColumn[x].Count; i++)
                {
                    var info = movesByColumn[x][i];

                    if (info.HasSlide)
                    {
                        var slideWorld = view.GridToWorld(info.SlideTargetCell);
                        var finalWorld = view.GridToWorld(info.TargetCell);

                        var slideDurationMultiplier = 1f + 2f * _shiftDurationMultiplier;
                        var fallDurationMultiplier = CalcFallDurMul(slideWorld.y, finalWorld.y, cellSize);

                        var seq = DOTween.Sequence();
                        seq.Append(info.grid.ItemAnimation.Shift(slideWorld, slideDurationMultiplier, swipeDelay));
                        var fallTween = info.grid.ItemAnimation.Shift(finalWorld, fallDurationMultiplier, 0f);
                        seq.Append(fallTween);

                        swipeDelay += fallTween.Duration() * 0.75f;
                        tasks.Add(seq.AsyncWaitForCompletion().AsUniTask());
                    }
                    else
                    {
                        var finalWorld = view.GridToWorld(info.TargetCell);

                        var fallDurationMultiplier = CalcFallDurMul(info.StartWorld.y, finalWorld.y, cellSize);

                        var delay = waveIndex * _shiftDelayMultiplier;
                        waveIndex++;

                        var tween = info.grid.ItemAnimation.Shift(finalWorld, fallDurationMultiplier, delay);
                        tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                    }
                }
            }
        }

        private float CalcFallDurMul(float fromY, float toY, float cellSize)
        {
            var dist = Mathf.Abs(toY - fromY) / cellSize;
            return 1f + dist * _shiftDurationMultiplier;
        }
    }
}