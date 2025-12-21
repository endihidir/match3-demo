using System.Collections.Generic;
using Core.Item;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SlideDownFillStrategy : IGridFillStrategy
    {
        private const float WaveDelayStep = 0.05f;
        private const float FallDistanceMultiplier = 0.2f;
        private const float SpawnYOffset = 1.25f;

        private readonly List<Vector2Int> _spawnCells = new();

        private struct MoveInfo
        {
            public BaseItemObject Item;
            public Vector3 StartWorld;
            public Vector2Int TargetCell;
            public bool HasSlide;
            public Vector2Int SlideTargetCell;
        }

        public async UniTask Execute(GridContext context, List<UniTask> tasks, float startDelay = 0f)
        {
            await UniTask.WaitForSeconds(startDelay);
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            var movedSet = new HashSet<BaseItemObject>(width * height);
            var startWorldByItem = new Dictionary<BaseItemObject, Vector3>(width * height);
            var slideStepByItem = new Dictionary<BaseItemObject, Vector2Int>(width * height);

            SimulateGravityAndSlides(model, width, height, movedSet, startWorldByItem, slideStepByItem);
            SpawnRefill(context, view, width, height, cellSize, movedSet, startWorldByItem);

            var finalCellByItem = BuildFinalCellMap(model, width, height, movedSet);
            PlayMoveAnimations(view, width, cellSize, movedSet, startWorldByItem, finalCellByItem, slideStepByItem, tasks);

            await UniTask.CompletedTask;
        }

        private void SimulateGravityAndSlides(IGridModel model, int width, int height, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem, Dictionary<BaseItemObject, Vector2Int> slideStepByItem)
        {
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

                        if (TryFillEmptyCell(model, emptyCell, height, movedSet, startWorldByItem, slideStepByItem))
                            anyMoved = true;
                    }
                }
            }
        }

        private bool IsEmptyActiveCell(IGridModel model, Vector2Int cell)
        {
            if (!model.IsCellActive(cell)) return false;
            return !model.GetGridObject(cell);
        }

        private bool TryFillEmptyCell(IGridModel model, Vector2Int emptyCell, int height, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem, Dictionary<BaseItemObject, Vector2Int> slideStepByItem)
        {
            return TryFallIntoCell(model, emptyCell, movedSet, startWorldByItem) ||
                   TrySlideIntoTopGap(model, emptyCell, height, movedSet, startWorldByItem, slideStepByItem);
        }

        private bool TryFallIntoCell(IGridModel model, Vector2Int emptyCell, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem)
        {
            if (!model.TryFindVerticalSource(emptyCell.x, emptyCell.y, out var sourceCell)) return false;

            var item = model.GetGridObject(sourceCell);
            if (!item || item.IsStationary) return false;

            TrackMovedItem(item, movedSet, startWorldByItem);

            model.SetGridObject(emptyCell, item);
            model.SetGridObject(sourceCell, null);

            return true;
        }

        private bool TrySlideIntoTopGap(IGridModel model, Vector2Int emptyCell, int height, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem, Dictionary<BaseItemObject, Vector2Int> slideStepByItem)
        {
            if (!model.TryGetBarrierYAbove(emptyCell.x, emptyCell.y, out var barrierY)) return false;

            var topGapY = barrierY + 1;
            if (topGapY >= height) return false;
            if (emptyCell.y != topGapY) return false;
            var topGapCell = new Vector2Int(emptyCell.x, topGapY);

            if (!IsEmptyActiveCell(model, topGapCell)) return false;

            return TrySlideFromSideIntoCell(model, topGapCell, emptyCell.x + 1, barrierY, movedSet, startWorldByItem, slideStepByItem) ||
                   TrySlideFromSideIntoCell(model, topGapCell, emptyCell.x - 1, barrierY, movedSet, startWorldByItem, slideStepByItem);
        }

        private bool TrySlideFromSideIntoCell(IGridModel model, Vector2Int targetCell, int sideX, int sourceY, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem, Dictionary<BaseItemObject, Vector2Int> slideStepByItem)
        {
            if (sideX < 0 || sideX >= model.Width) return false;

            var sideCell = new Vector2Int(sideX, sourceY);
            if (!model.IsCellActive(sideCell)) return false;

            var item = model.GetGridObject(sideCell);
            if (!item) return false;
            if (item.IsStationary) return false;
            //if (model.CanFallStraightDown(sideCell)) return false;

            TrackMovedItem(item, movedSet, startWorldByItem);
            model.SetGridObject(targetCell, item);
            model.SetGridObject(sideCell, null);
            slideStepByItem[item] = targetCell;

            return true;
        }

        private void TrackMovedItem(BaseItemObject item, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem)
        {
            movedSet.Add(item);

            if (!startWorldByItem.ContainsKey(item))
                startWorldByItem.Add(item, item.transform.position);
        }

        private void SpawnRefill(GridContext context, IGridView view, int width, int height, float cellSize, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem)
        {
            for (int x = 0; x < width; x++)
            {
                if (!TryGetSpawnYForColumn(context.Model, view, x, height, cellSize, out var spawnY))
                    continue;

                SpawnTopOpenCells(context, view, x, height, cellSize, spawnY, movedSet, startWorldByItem);
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

        private void SpawnTopOpenCells(GridContext context, IGridView view, int x, int height, float cellSize, float spawnY, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem)
        {
            var model = context.Model;

            _spawnCells.Clear();

            var blockedBelow = false;

            for (int y = 0; y < height; y++)
            {
                var cell = new Vector2Int(x, y);

                if (!model.IsCellActive(cell)) continue;

                var existing = model.GetGridObject(cell);

                if (existing && existing.IsStationary)
                {
                    blockedBelow = true;
                    continue;
                }

                if (blockedBelow) continue;
                if (existing) continue;

                _spawnCells.Add(cell);
            }

            for (int i = 0; i < _spawnCells.Count; i++)
            {
                var cell = _spawnCells[i];
                var item = context.Factory.GetRandomItem();

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var targetWorld = view.GridToWorld(cell);
                item.SetPosition(new Vector3(targetWorld.x, spawnY, targetWorld.z));

                model.SetGridObject(cell, item);
                TrackMovedItem(item, movedSet, startWorldByItem);
            }
        }

        private Dictionary<BaseItemObject, Vector2Int> BuildFinalCellMap(IGridModel model, int width, int height, HashSet<BaseItemObject> movedSet)
        {
            var map = new Dictionary<BaseItemObject, Vector2Int>(movedSet.Count);

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

        private void PlayMoveAnimations(IGridView view, int width, float cellSize, HashSet<BaseItemObject> movedSet, Dictionary<BaseItemObject, Vector3> startWorldByItem, Dictionary<BaseItemObject, Vector2Int> finalCellByItem, Dictionary<BaseItemObject, Vector2Int> slideStepByItem, List<UniTask> tasks)
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
                    Item = item,
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

                        var slideDurationMultiplier = 1f + 2f * FallDistanceMultiplier;
                        var fallDurationMultiplier = CalcFallDurMul(slideWorld.y, finalWorld.y, cellSize);

                        var seq = DOTween.Sequence();
                        seq.Append(info.Item.ItemAnimation.Shift(slideWorld, slideDurationMultiplier, swipeDelay));
                        var fallTween = info.Item.ItemAnimation.Shift(finalWorld, fallDurationMultiplier, 0f);
                        seq.Append(fallTween);

                        swipeDelay += fallTween.Duration() * 0.75f;
                        tasks.Add(seq.AsyncWaitForCompletion().AsUniTask());
                    }
                    else
                    {
                        var finalWorld = view.GridToWorld(info.TargetCell);

                        var fallDurationMultiplier = CalcFallDurMul(info.StartWorld.y, finalWorld.y, cellSize);

                        var delay = waveIndex * WaveDelayStep;
                        waveIndex++;

                        var tween = info.Item.ItemAnimation.Shift(finalWorld, fallDurationMultiplier, delay);
                        tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                    }
                }
            }
        }

        private float CalcFallDurMul(float fromY, float toY, float cellSize)
        {
            var dist = Mathf.Abs(toY - fromY) / cellSize;
            return 1f + dist * FallDistanceMultiplier;
        }
    }
}