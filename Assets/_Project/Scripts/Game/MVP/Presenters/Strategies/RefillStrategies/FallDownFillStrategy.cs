using System.Collections.Generic;
using Core.Config;
using Core.Models;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class FallDownRefillStrategy : IRefillStrategy
    {
        private RefillSettings _refillSettings;
        public bool CanRefill(IGridModel model) => !GridRefillCalc.HasStationaryAndBlocking(model);
        
        public void SetRefillSettings(RefillSettings refillSettings)
        {
            _refillSettings = refillSettings;
        }

        public async UniTask Execute(GridStateContext context, List<UniTask> tasks)
        {
            await UniTask.WaitForSeconds(_refillSettings.RefillStartDelay);
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                var wave = 0;

                ShiftColumn(context, x, height, cellSize, ref wave, tasks);

                if (GridRefillCalc.TryGetSpawnCell(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumn(context, x, height, cellSize, spawnY, ref wave, tasks);
                }
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

                var srcY = GridRefillCalc.FindFallSourceY(stateContext.Model, x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var obj = model.GetGridObject(src);

                if (!obj) continue;

                var startWorld = obj.transform.position;
                var finalWorld = view.GridToWorld(dest);

                model.SetGridObject(dest, obj);
                model.SetGridObject(src, null);

                var dist = Mathf.Abs(finalWorld.y - startWorld.y) / cellSize;
                var durMul = 1f + dist * _refillSettings.ShiftDurationMultiplier;
                var delay = wave * _refillSettings.ShiftDelayMultiplier;

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
                var durMul = 1f + dist * _refillSettings.ShiftDurationMultiplier;
                var delay = wave * _refillSettings.ShiftDelayMultiplier;

                var tween = item.ItemAnimation.Shift(finalWorld, durMul, delay);
                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                wave++;
            }
        }
    }
}