using System.Collections.Generic;
using Core.Config;
using Core.Models;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class FallDownRefillStrategy : IRefillStrategy
    {
        private float SpawnYOffset = 1.25f;

        private float _startDelay = 0f;
        private float _shiftDurationMultiplier = 0.2f;
        private float _shiftDelayMultiplier = 0.05f;
        public bool CanRefill(IGridModel model) => !model.HasStationaryAndBlocking();
        
        public void SetRefillSettings(RefillSettings refillSettings)
        {
            _startDelay = refillSettings.RefillStartDelay;
            _shiftDurationMultiplier = refillSettings.ShiftDurationMultiplier;
            _shiftDelayMultiplier =  refillSettings.ShiftDelayMultiplier;
        }

        public async UniTask Execute(GridStateContext stateContext, List<UniTask> tasks)
        {
            await UniTask.WaitForSeconds(_startDelay);
            var model = stateContext.Model;
            var view = stateContext.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                var wave = 0;

                ShiftColumn(stateContext, x, height, cellSize, ref wave, tasks);

                if (TryGetSpawnY(model, view, x, height, cellSize, out var spawnY))
                    RefillColumn(stateContext, x, height, cellSize, spawnY, ref wave, tasks);
            }

            await UniTask.CompletedTask;
        }

        private void ShiftColumn(GridStateContext stateContext, int x, int height, float cellSize, ref int wave, List<UniTask> tasks)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            for (int y = height - 1; y >= 0; y--)
            {
                var dest = new Vector2Int(x, y);

                if (!model.IsCellActive(dest)) continue;
                if (model.GetGridObject(dest)) continue;

                var srcY = model.FindFallSourceY(x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var obj = model.GetGridObject(src);

                if (!obj) continue;

                var startWorld = obj.transform.position;
                var finalWorld = view.GridToWorld(dest);

                model.SetGridObject(dest, obj);
                model.SetGridObject(src, null);

                var dist = Mathf.Abs(finalWorld.y - startWorld.y) / cellSize;
                var durMul = 1f + dist * _shiftDurationMultiplier;
                var delay = wave * _shiftDelayMultiplier;

                var tween = obj.ItemAnimation.Shift(finalWorld, durMul, delay);
                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());

                wave++;
            }
        }

        private void RefillColumn(GridStateContext stateContext, int x, int height, float cellSize, float spawnY, ref int wave, List<UniTask> tasks)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            for (int y = height - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);

                if (!model.IsCellActive(pos)) continue;
                if (model.GetGridObject(pos)) continue;
                
                var item = stateContext.Factory.GetRandomItem();

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var target = view.GridToWorld(pos);
                var start = new Vector3(target.x, spawnY, target.z);

                item.SetPosition(start);
                model.SetGridObject(pos, item);

                var finalWorld = target;

                var dist = Mathf.Abs(finalWorld.y - start.y) / cellSize;
                var durMul = 1f + dist * _shiftDurationMultiplier;
                var delay = wave * _shiftDelayMultiplier;

                var tween = item.ItemAnimation.Shift(finalWorld, durMul, delay);
                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                wave++;
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