using System.Collections.Generic;
using Core.Config;
using Core.Configs;
using Core.Models;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class FallDownRefillStrategy : IRefillStrategy
    {
        private readonly RefillSettingsSO _refillSettingsSo;
        private readonly List<UniTask> _refillTasks = new(128);
        public bool CanRefill(IGridModel model) => !GridRefillCalcUtils.HasStationaryAndBlocking(model);

        public FallDownRefillStrategy(GameplayConfigContainer configContainer)
        {
            _refillSettingsSo = configContainer.ItemConfigContainerSo.RefillSettingsSo;
        }

        public async UniTask Execute(GridStateContext context)
        {
            _refillTasks.Clear();
            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;

            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                var wave = 0;

                ShiftColumn(context, x, height, cellSize, ref wave, _refillTasks);

                if (GridRefillCalcUtils.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumn(context, x, height, cellSize, spawnY, ref wave, _refillTasks);
                }
            }

            await UniTask.WhenAll(_refillTasks);
        }

        private void ShiftColumn(GridStateContext stateContext, int x, int height, float cellSize, ref int wave, List<UniTask> tasks)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;

                var srcY = GridRefillCalcUtils.FindFallSourceY(stateContext.Model, x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var obj = model.GetGridObject(src);

                if (!obj) continue;

                var startWorld = obj.transform.position;
                var finalWorld = view.GridToWorld(coord);

                model.SetGridObject(coord, obj);
                model.SetGridObject(src, null);

                var dist = Mathf.Abs(finalWorld.y - startWorld.y) / cellSize;
                var durMul = 1f + dist * _refillSettingsSo.ShiftDurationMultiplier;
                var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

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
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;
                
                var itemType = SmartSpawnDecider.Decide(model, coord, _refillSettingsSo.SpawnSettings);
                var item = stateContext.Factory.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var target = view.GridToWorld(coord);
                var start = new Vector3(target.x, spawnY, target.z);

                item.SetPosition(start);
                model.SetGridObject(coord, item);

                var finalWorld = target;

                var dist = Mathf.Abs(finalWorld.y - start.y) / cellSize;
                var durMul = 1f + dist * _refillSettingsSo.ShiftDurationMultiplier;
                var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

                var tween = item.ItemAnimation.Shift(finalWorld, durMul, delay);
                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
                wave++;
            }
        }
    }
}