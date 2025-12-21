using System.Collections.Generic;
using Core.Item;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class FallDownFillStrategy : IGridFillStrategy
    {
        private const float WaveDelayStep = 0.05f;
        private const float FallDistanceMultiplier = 0.2f;
        private const float SpawnYOffset = 1.25f;

        private struct MoveInfo
        {
            public BaseItemObject Item;
            public Vector3 StartWorld;
            public Vector2Int FinalGrid;
        }

        public async UniTask Execute(GridContext context, List<UniTask> tasks)
        {
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            var movedOrSpawned = new HashSet<BaseItemObject>(width * height);
            var startWorld = new Dictionary<BaseItemObject, Vector3>(width * height);

            ApplyVerticalGravity_SetOnly(model, width, height, movedOrSpawned, startWorld);

            for (int x = 0; x < width; x++)
            {
                if (!TryGetSpawnY(model, view, x, height, cellSize, out var spawnY)) continue;
                RefillTopOpenSegments_SetOnly(context, x, height, cellSize, spawnY, movedOrSpawned, startWorld);
            }

            var finalGrid = BuildFinalGridMap(model, width, height, movedOrSpawned);
            PlayMovesByColumn(view, width, cellSize, movedOrSpawned, startWorld, finalGrid, tasks);

            await UniTask.CompletedTask;
        }

        private void ApplyVerticalGravity_SetOnly(IGridModel model, int width, int height, HashSet<BaseItemObject> movedOrSpawned, Dictionary<BaseItemObject, Vector3> startWorld)
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

                        if (!TryFindVerticalSource(model, x, y, out var src)) continue;

                        var obj = model.GetGridObject(src);
                        if (!obj || obj.IsStationary) continue;

                        if (!startWorld.ContainsKey(obj))
                            startWorld.Add(obj, obj.transform.position);

                        movedOrSpawned.Add(obj);

                        model.SetGridObject(dest, obj);
                        model.SetGridObject(src, null);

                        moved = true;
                    }
                }
            }
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

        private void RefillTopOpenSegments_SetOnly(GridContext context, int x, int height, float cellSize, float spawnY, HashSet<BaseItemObject> movedOrSpawned, Dictionary<BaseItemObject, Vector3> startWorld)
        {
            var model = context.Model;
            var view = context.View;

            var spawnPositions = new List<Vector2Int>();

            for (int y = 0; y < height; y++)
            {
                var pos = new Vector2Int(x, y);
                if (!model.IsCellActive(pos)) continue;

                var existing = model.GetGridObject(pos);
                if (existing && existing.IsStationary) break;
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

                movedOrSpawned.Add(item);

                if (!startWorld.ContainsKey(item))
                    startWorld.Add(item, item.transform.position);
            }
        }

        private Dictionary<BaseItemObject, Vector2Int> BuildFinalGridMap(IGridModel model, int width, int height, HashSet<BaseItemObject> movedOrSpawned)
        {
            var map = new Dictionary<BaseItemObject, Vector2Int>(movedOrSpawned.Count);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!model.IsCellActive(pos)) continue;

                    var obj = model.GetGridObject(pos);
                    if (!obj) continue;

                    if (!movedOrSpawned.Contains(obj)) continue;

                    if (!map.ContainsKey(obj))
                        map.Add(obj, pos);
                }
            }

            return map;
        }

        private void PlayMovesByColumn(IGridView view, int width, float cellSize, HashSet<BaseItemObject> movedOrSpawned, Dictionary<BaseItemObject, Vector3> startWorld, Dictionary<BaseItemObject, Vector2Int> finalGrid, List<UniTask> tasks)
        {
            var byColumn = new List<MoveInfo>[width];

            for (int x = 0; x < width; x++)
                byColumn[x] = new List<MoveInfo>();

            foreach (var item in movedOrSpawned)
            {
                if (!item) continue;
                if (!finalGrid.TryGetValue(item, out var grid)) continue;

                if (!startWorld.TryGetValue(item, out var start))
                    start = item.transform.position;

                byColumn[grid.x].Add(new MoveInfo
                {
                    Item = item,
                    StartWorld = start,
                    FinalGrid = grid
                });
            }

            for (int x = 0; x < width; x++)
            {
                byColumn[x].Sort((a, b) => b.FinalGrid.y.CompareTo(a.FinalGrid.y));

                for (int i = 0; i < byColumn[x].Count; i++)
                {
                    var info = byColumn[x][i];

                    var finalWorld = view.GridToWorld(info.FinalGrid);

                    var dist = Mathf.Abs(finalWorld.y - info.StartWorld.y) / cellSize;
                    var durMul = 1f + dist * FallDistanceMultiplier;

                    var delay = i * WaveDelayStep;

                    var tween = info.Item.ItemAnimation.Shift(finalWorld, durMul, delay);
                    if (tween != null)
                        tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                }
            }
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
    }
}