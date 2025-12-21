using System.Collections.Generic;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SimpleFillStrategy : IGridFillStrategy
    {
        private const float WaveDelayStep = 0.05f;
        private const float FallDistanceMultiplier = 0.2f;
        private const float SpawnYOffset = 1.25f;

        public async UniTask Execute(GridContext context, List<UniTask> tasks)
        {
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                if (!TryGetSpawnY(model, view, x, height, cellSize, out var spawnY)) continue;

                var spawnPositions = new List<Vector2Int>();

                for (int y = 0; y < height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!model.IsCellActive(pos)) continue;

                    var existing = model.GetGridObject(pos);
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

                    var delayIndex = spawnPositions.Count - 1 - i;
                    var delay = delayIndex * WaveDelayStep;

                    var dist = Mathf.Abs(spawnY - target.y) / cellSize;
                    var durMul = 1f + dist * FallDistanceMultiplier;

                    var tween = item.ItemAnimation.Shift(target, durMul, delay);
                    if (tween != null)
                        tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                }
            }

            await UniTask.CompletedTask;
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