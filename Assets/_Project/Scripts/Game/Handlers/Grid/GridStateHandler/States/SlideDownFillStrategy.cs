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

        private struct MoveInfo
        {
            public BaseItemObject Item;
            public Vector3 StartWorld;
            public Vector2Int FinalGrid;
            public bool HasSlide;
            public Vector2Int SlideGrid;
            public bool IsSpawn;
        }

        public async UniTask Execute(GridContext context, List<UniTask> tasks)
        {
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            var touched = new HashSet<BaseItemObject>(width * height);
            var startWorld = new Dictionary<BaseItemObject, Vector3>(width * height);
            var slideGrid = new Dictionary<BaseItemObject, Vector2Int>(width * height);
            var isSpawn = new HashSet<BaseItemObject>(width * height);

            ApplyGravityAndSlides_SetOnly(model, width, height, touched, startWorld, slideGrid);

            for (int x = 0; x < width; x++)
            {
                if (!TryGetSpawnY(model, view, x, height, cellSize, out var spawnY)) continue;
                RefillTopOpenSegments_SetOnly(context, x, height, cellSize, spawnY, touched, startWorld, isSpawn);
            }

            var finalGrid = BuildFinalGridMap(model, width, height, touched);
            PlayMoves(view, width, cellSize, touched, startWorld, finalGrid, slideGrid, isSpawn, tasks);

            await UniTask.CompletedTask;
        }

        private void ApplyGravityAndSlides_SetOnly(IGridModel model, int width, int height, HashSet<BaseItemObject> touched, Dictionary<BaseItemObject, Vector3> startWorld, Dictionary<BaseItemObject, Vector2Int> slideGrid)
        {
            var moved = true;
            var safety = width * height * 12;

            while (moved && safety-- > 0)
            {
                moved = false;

                for (int y = height - 1; y >= 0; y--)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var dest = new Vector2Int(x, y);

                        if (!model.IsCellActive(dest)) continue;
                        if (model.GetGridObject(dest)) continue;

                        if (TryFindVerticalSource(model, x, y, out var vSrc))
                        {
                            var obj = model.GetGridObject(vSrc);
                            if (!obj || obj.IsStationary) continue;

                            Touch(obj, touched, startWorld);

                            model.SetGridObject(dest, obj);
                            model.SetGridObject(vSrc, null);

                            moved = true;
                            continue;
                        }

                        if (!TryGetBarrierYAbove(model, x, y, out var barrierY))
                            continue;

                        var topGapY = barrierY + 1;
                        if (topGapY >= height)
                            continue;

                        if (y != topGapY)
                            continue;

                        var topGap = new Vector2Int(x, topGapY);

                        if (!model.IsCellActive(topGap))
                            continue;

                        if (model.GetGridObject(topGap))
                            continue;

                        if (TrySlideOneStepFromSide_SetOnly(model, topGap, height, sideX: x + 1, srcY: barrierY, touched, startWorld, slideGrid))
                        {
                            moved = true;
                            continue;
                        }

                        if (TrySlideOneStepFromSide_SetOnly(model, topGap, height, sideX: x - 1, srcY: barrierY, touched, startWorld, slideGrid))
                        {
                            moved = true;
                        }
                    }
                }
            }
        }

        private void Touch(BaseItemObject obj, HashSet<BaseItemObject> touched, Dictionary<BaseItemObject, Vector3> startWorld)
        {
            touched.Add(obj);

            if (!startWorld.ContainsKey(obj))
                startWorld.Add(obj, obj.transform.position);
        }

        private bool TrySlideOneStepFromSide_SetOnly(IGridModel model, Vector2Int target, int height, int sideX, int srcY, HashSet<BaseItemObject> touched, Dictionary<BaseItemObject, Vector3> startWorld, Dictionary<BaseItemObject, Vector2Int> slideGrid)
        {
            if (sideX < 0 || sideX >= model.Width)
                return false;

            var src = new Vector2Int(sideX, srcY);
            if (!model.IsCellActive(src))
                return false;

            var obj = model.GetGridObject(src);
            if (!obj)
                return false;

            if (obj.IsStationary)
                return false;

            if (CanFallStraightDown(model, src, height))
                return false;

            Touch(obj, touched, startWorld);

            model.SetGridObject(target, obj);
            model.SetGridObject(src, null);

            slideGrid[obj] = target;

            return true;
        }

        private bool TryFindVerticalSource(IGridModel model, int x, int destY, out Vector2Int src)
        {
            for (int y = destY - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);

                if (!model.IsCellActive(pos)) continue;

                var obj = model.GetGridObject(pos);
                if (!obj) continue;

                if (obj.IsStationary)
                    break;

                src = pos;
                return true;
            }

            src = default;
            return false;
        }

        private bool TryGetBarrierYAbove(IGridModel model, int x, int destY, out int barrierY)
        {
            for (int y = destY - 1; y >= 0; y--)
            {
                var p = new Vector2Int(x, y);
                if (!model.IsCellActive(p)) continue;

                var obj = model.GetGridObject(p);
                if (obj && obj.IsStationary)
                {
                    barrierY = y;
                    return true;
                }
            }

            barrierY = -1;
            return false;
        }

        private bool CanFallStraightDown(IGridModel model, Vector2Int pos, int height)
        {
            var below = new Vector2Int(pos.x, pos.y + 1);
            if (below.y >= height) return false;
            if (!model.IsCellActive(below)) return false;

            return !model.GetGridObject(below);
        }

        private bool TryGetSpawnY(IGridModel model, IGridView view, int x, int height, float cellSize, out float spawnY)
        {
            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2Int(x, y);
                if (!model.IsCellActive(pos)) continue;

                spawnY = view.GridToWorld(pos).y + cellSize * SpawnYOffset;
                return true;
            }

            spawnY = 0f;
            return false;
        }

        private void RefillTopOpenSegments_SetOnly(GridContext context, int x, int height, float cellSize, float spawnY, HashSet<BaseItemObject> touched, Dictionary<BaseItemObject, Vector3> startWorld, HashSet<BaseItemObject> isSpawn)
        {
            var model = context.Model;
            var view = context.View;

            var spawnPositions = new List<Vector2Int>();
            var blockedBelow = false;

            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2Int(x, y);
                if (!model.IsCellActive(pos)) continue;

                var existing = model.GetGridObject(pos);
                if (existing && existing.IsStationary)
                {
                    blockedBelow = true;
                    continue;
                }

                if (blockedBelow) continue;
                if (existing) continue;

                spawnPositions.Add(pos);
            }

            for (int i = 0; i < spawnPositions.Count; i++)
            {
                var pos = spawnPositions[i];
                var item = context.Factory.GetRandomItem();

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var target = view.GridToWorld(pos);
                item.SetPosition(new Vector3(target.x, spawnY, target.z));

                model.SetGridObject(pos, item);

                Touch(item, touched, startWorld);
                isSpawn.Add(item);
            }
        }

        private Dictionary<BaseItemObject, Vector2Int> BuildFinalGridMap(IGridModel model, int width, int height, HashSet<BaseItemObject> touched)
        {
            var map = new Dictionary<BaseItemObject, Vector2Int>(touched.Count);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!model.IsCellActive(pos)) continue;

                    var obj = model.GetGridObject(pos);
                    if (!obj) continue;

                    if (!touched.Contains(obj)) continue;

                    map.TryAdd(obj, pos);
                }
            }

            return map;
        }

        private void PlayMoves(IGridView view, int width, float cellSize, HashSet<BaseItemObject> touched, Dictionary<BaseItemObject, Vector3> startWorld, Dictionary<BaseItemObject, Vector2Int> finalGrid, Dictionary<BaseItemObject, Vector2Int> slideGrid, HashSet<BaseItemObject> isSpawn, List<UniTask> tasks)
        {
            var byColumn = new List<MoveInfo>[width];

            for (int x = 0; x < width; x++)
                byColumn[x] = new List<MoveInfo>();

            foreach (var item in touched)
            {
                if (!item) continue;
                if (!finalGrid.TryGetValue(item, out var grid)) continue;

                if (!startWorld.TryGetValue(item, out var start))
                    start = item.transform.position;

                var hasSlide = slideGrid.TryGetValue(item, out var sGrid);

                byColumn[grid.x].Add(new MoveInfo
                {
                    Item = item,
                    StartWorld = start,
                    FinalGrid = grid,
                    HasSlide = hasSlide,
                    SlideGrid = sGrid,
                    IsSpawn = isSpawn.Contains(item)
                });
            }

            var swipeDelay = 0f;
            
            for (int x = 0; x < width; x++)
            {
                byColumn[x].Sort((a, b) => b.FinalGrid.y.CompareTo(a.FinalGrid.y));
                
                var wave = 0;

                for (int i = 0; i < byColumn[x].Count; i++)
                {
                    var info = byColumn[x][i];

                    Tween tween;

                    if (info.HasSlide)
                    {
                        var slideWorld = view.GridToWorld(info.SlideGrid);
                        var finalWorld = view.GridToWorld(info.FinalGrid);

                        var durMul1 = 1f + 2f * FallDistanceMultiplier;

                        var dist2 = Mathf.Abs(finalWorld.y - slideWorld.y) / cellSize;
                        var durMul2 = 1f + dist2 * FallDistanceMultiplier;

                        var seq = DOTween.Sequence();
                        seq.Append(info.Item.ItemAnimation.Shift(slideWorld, durMul1, swipeDelay));
                        var shift = info.Item.ItemAnimation.Shift(finalWorld, durMul2);
                        seq.Append(shift);
                        swipeDelay += shift.Duration() * 0.75f;
                        tween = seq;
                    }
                    else
                    {
                        var finalWorld = view.GridToWorld(info.FinalGrid);
                        var dist = Mathf.Abs(finalWorld.y - info.StartWorld.y) / cellSize;
                        var durMul = 1f + dist * FallDistanceMultiplier;
                        var delay = wave * WaveDelayStep;
                        wave++;

                        tween = info.Item.ItemAnimation.Shift(finalWorld, durMul, delay);
                    }

                    if (tween != null)
                        tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                }
            }
        }
    }
}