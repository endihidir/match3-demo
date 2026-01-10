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
    public sealed class FallDownFillStrategy : IFillStrategy
    {
        private readonly RefillSettingsSO _refillSettingsSo;

        private readonly List<FallDownMoveRecord> _records = new(256);
        private Task[] _animTasks = new Task[128];
        private Task _runningAnimations;

        public bool CanRefill(IGridModel model) => !GridRefillCalcUtil.HasStationaryAndBlocking(model);

        public FallDownFillStrategy(GameplayConfigContainer configContainer)
        {
            _refillSettingsSo = configContainer.RefillSettings;
        }

        public IFillStrategy Execute(GridStateContext context)
        {
            _records.Clear();

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            for (int x = 0; x < width; x++)
            {
                ShiftColumnLogic(context, x, height, _records);

                if (GridRefillCalcUtil.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumnLogic(context, x, height, cellSize, spawnY, _records);
                }
            }

            _runningAnimations = PlayAnimations(context, _records);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations.AsUniTask();

        private void ShiftColumnLogic(GridStateContext stateContext, int x, int height, List<FallDownMoveRecord> records)
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

                var finalWorld = view.GridToWorld(coord);

                model.SetGridObject(coord, obj);
                model.SetGridObject(src, null);

                records.Add(new FallDownMoveRecord(obj, finalWorld, x, false));
            }
        }

        private void RefillColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, float spawnY, List<FallDownMoveRecord> records)
        {
            var model = stateContext.Model;
            var view = stateContext.View;

            var stack = 0;

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
                var start = new Vector3(target.x, spawnY + (stack * cellSize), target.z);

                item.SetPosition(start);
                model.SetGridObject(coord, item);

                records.Add(new FallDownMoveRecord(item, target, x, true));

                stack++;
            }
        }

        private Task PlayAnimations(GridStateContext context, List<FallDownMoveRecord> records)
        {
            var view = context.View;
            var cellSize = view.GetCellSize();
            var width = context.Model.Width;

            if (_animTasks.Length < records.Count)
                Array.Resize(ref _animTasks, records.Count);

            var taskCount = 0;

            for (int x = 0; x < width; x++)
            {
                var wave = 0;

                for (int i = 0; i < records.Count; i++)
                {
                    var r = records[i];
                    if (!r.Obj) continue;
                    if (r.ColumnX != x) continue;
                    if (r.IsSpawn) continue;

                    var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

                    var startWorld = r.Obj.transform.position;
                    var distCells = Mathf.Abs(r.FinalWorld.y - startWorld.y) / cellSize;

                    var tween = r.Obj.ItemAnimation.Shift(r.FinalWorld, distCells, delay);
                    _animTasks[taskCount++] = tween.AsyncWaitForCompletion();

                    wave++;
                }

                for (int i = 0; i < records.Count; i++)
                {
                    var r = records[i];
                    if (!r.Obj) continue;
                    if (r.ColumnX != x) continue;
                    if (!r.IsSpawn) continue;

                    var delay = wave * _refillSettingsSo.ShiftDelayMultiplier;

                    var startWorld = r.Obj.transform.position;
                    var distCells = Mathf.Abs(r.FinalWorld.y - startWorld.y) / cellSize;

                    var tween = r.Obj.ItemAnimation.Shift(r.FinalWorld, distCells, delay);
                    _animTasks[taskCount++] = tween.AsyncWaitForCompletion();

                    wave++;
                }
            }

            return taskCount == 0 ? Task.CompletedTask : Task.WhenAll(_animTasks.AsSpan(0, taskCount).ToArray());
        }
    }
}