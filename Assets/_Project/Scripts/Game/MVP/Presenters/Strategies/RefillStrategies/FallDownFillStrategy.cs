using System;
using System.Collections.Generic;
using Core.Models;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class FallDownFillStrategy : IFillStrategy
    {
        private readonly IFillItemDecider _itemDecider;
        private readonly IShiftAnimationScheduler _shiftAnimationScheduler;

        private readonly List<FallDownMoveRecord> _records = new(256);

        private ColumnWaveState[] _waveByX = Array.Empty<ColumnWaveState>();
        private UniTask[] _animTasks = new UniTask[128];
        private UniTask _runningAnimations;

        public bool CanHandle(IGridModel model) => !GridFillCalcUtil.HasStationaryAndBlocking(model);

        public FallDownFillStrategy(IFillItemDecider itemDecider, IShiftAnimationScheduler shiftAnimationScheduler)
        {
            _itemDecider = itemDecider;
            _shiftAnimationScheduler = shiftAnimationScheduler;
        }

        public IFillStrategy Execute(GridStateContext context)
        {
            _records.Clear();

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            EnsureBuffers(width);

            for (int x = 0; x < width; x++)
            {
                ShiftColumnLogic(model, x, height, _records);

                if (GridFillCalcUtil.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumnLogic(context, x, height, cellSize, spawnY, _records);
                }
            }

            _runningAnimations = PlayAnimations(context, _records);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        private void EnsureBuffers(int width)
        {
            if (_waveByX.Length < width)
                _waveByX = new ColumnWaveState[width];
        }

        private void ShiftColumnLogic(IGridModel model, int x, int height, List<FallDownMoveRecord> records)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (model.GetGridObject(coord)) continue;

                var srcY = GridFillCalcUtil.FindFallSourceY(model, x, y - 1);
                if (srcY < 0) continue;

                var src = new Vector2Int(x, srcY);
                var item = model.GetGridObject(src);

                if (!item) continue;
                

                model.SetGridObject(coord, item);
                model.SetGridObject(src, null);
                
                var fallMoveRecord = new FallDownMoveRecord(item, coord, false);
                records.Add(fallMoveRecord);
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

                var itemType = _itemDecider.Decide(model, coord);
                var item = stateContext.Factory.GetRegularItem(itemType);

                item.SetParent(view.GridObjectsParent);
                item.SetSpriteSize(cellSize);

                var worldPos = view.GridToWorld(coord);
                var startY = spawnY + (stack * cellSize);
                var start = new Vector3(worldPos.x, startY, worldPos.z);

                item.SetPosition(start);
                model.SetGridObject(coord, item);

                var fallMoveRecord = new FallDownMoveRecord(item, coord, true);
                records.Add(fallMoveRecord);
                stack++;
            }
        }

        private UniTask PlayAnimations(GridStateContext context, List<FallDownMoveRecord> records)
        {
            var view = context.View;
            var width = context.Model.Width;

            if (_animTasks.Length < records.Count)
                Array.Resize(ref _animTasks, records.Count);

            Array.Clear(_waveByX, 0, width);

            var taskCount = 0;

            ScheduleShift(view, width, records, false, _animTasks, ref taskCount);
            ScheduleShift(view, width, records, true, _animTasks, ref taskCount);

            return taskCount == 0 ? UniTask.CompletedTask : UniTask.WhenAll(_animTasks.AsSpan(0, taskCount).ToArray());
        }
        
        private void ScheduleShift(IGridView view, int width, List<FallDownMoveRecord> records, bool passIsSpawn, UniTask[] animTasks, ref int taskCount)
        {
            for (int x = 0; x < width; x++)
            {
                ref var state = ref _waveByX[x];

                for (int i = 0; i < records.Count; i++)
                {
                    var fallRecord = records[i];
                    if (!fallRecord.Item) continue;
                    if (fallRecord.FinalCoord.x != x) continue;
                    if (fallRecord.IsSpawn != passIsSpawn) continue;
                    _shiftAnimationScheduler.Schedule(view, fallRecord, animTasks, ref taskCount, ref state);
                }
            }
        }
    }
}
