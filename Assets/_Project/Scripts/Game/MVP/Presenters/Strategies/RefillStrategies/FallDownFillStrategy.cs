using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly List<FallMoveRecord> _records = new(256);
        private Task[] _animTasks = new Task[128];
        private Task _runningAnimations;

        public bool CanRefill(IGridModel model) => !GridRefillCalcUtil.HasStationaryAndBlocking(model);

        public FallDownRefillStrategy(GameplayConfigContainer configContainer)
        {
            _refillSettingsSo = configContainer.RefillSettings;
        }

        public IRefillStrategy Execute(GridStateContext context)
        {
            _records.Clear();

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                var wave = 0;

                ShiftColumnLogic(context, x, height, cellSize, ref wave, _records);

                if (GridRefillCalcUtil.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumnLogic(context, x, height, cellSize, spawnY, ref wave, _records);
                }
            }

            _runningAnimations = PlayAnimations(_records);
            return this;
        }
        
        public UniTask WaitAnimationsAsync() => _runningAnimations.AsUniTask();

        private void ShiftColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, ref int wave, List<FallMoveRecord> records)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;

                var srcY = GridRefillCalcUtil.FindFallSourceY(model, x, y - 1);
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

                records.Add(new FallMoveRecord(obj, finalWorld, durMul, delay));
                wave++;
            }
        }

        private void RefillColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, float spawnY, ref int wave, List<FallMoveRecord> records)
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

                var dist = Mathf.Abs(target.y - start.y) / cellSize;
                var durMul = .5f + dist * _refillSettingsSo.ShiftDurationMultiplier;
                var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

                records.Add(new FallMoveRecord(item, target, durMul, delay));
                wave++;
            }
        }

        private Task PlayAnimations(List<FallMoveRecord> records)
        {
            var length = records.Count;
            Array.Resize(ref _animTasks, length);
            
            for (int i = 0; i < length; i++)
            {
                var r = records[i];
                if (!r.Obj) continue;
                var tween = r.Obj.ItemAnimation.Shift(r.FinalWorld, r.DurMul, r.Delay);
                _animTasks[i] = tween.AsyncWaitForCompletion();
            }
            
            return length == 0 ? Task.CompletedTask : Task.WhenAll(_animTasks);
        }
    }
}
