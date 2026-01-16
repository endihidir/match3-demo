using System;
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

        private FallDownMoveRecord[] _records = Array.Empty<FallDownMoveRecord>();
        private int _recordCount;

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
            _recordCount = 0;

            var model = context.Model;
            var view = context.View;

            var width = model.Width;
            var height = model.Height;
            var cellSize = view.GetCellSize();

            EnsureBuffers(width);
            EnsureRecordCapacity(width * height);

            for (int x = 0; x < width; x++)
            {
                ShiftColumnLogic(model, x, height);

                if (GridFillCalcUtil.TryGetSpawnCellCoord(model, x, model.Height, out var spawnCell))
                {
                    var spawnY = view.GridToWorld(spawnCell).y + cellSize;
                    RefillColumnLogic(context, x, height, cellSize, spawnY);
                }
            }

            _runningAnimations = PlayAnimations(context);
            return this;
        }

        public UniTask WaitAnimationsAsync() => _runningAnimations;

        private void EnsureBuffers(int width)
        {
            if (_waveByX.Length < width)
                _waveByX = new ColumnWaveState[width];
        }

        private void EnsureRecordCapacity(int capacity)
        {
            if (_records.Length < capacity)
                Array.Resize(ref _records, capacity);
        }

        private void AddRecord(in FallDownMoveRecord record)
        {
            if (_recordCount >= _records.Length)
                Array.Resize(ref _records, _records.Length == 0 ? 256 : _records.Length * 2);

            _records[_recordCount++] = record;
        }

        private void ShiftColumnLogic(IGridModel model, int x, int height)
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
                AddRecord(fallMoveRecord);
            }
        }

        private void RefillColumnLogic(GridStateContext stateContext, int x, int height, float cellSize, float spawnY)
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
                AddRecord(fallMoveRecord);
                stack++;
            }
        }

        private UniTask PlayAnimations(GridStateContext context)
        {
            var view = context.View;
            var width = context.Model.Width;

            if (_animTasks.Length < _recordCount)
                Array.Resize(ref _animTasks, _recordCount);

            Array.Clear(_waveByX, 0, width);

            var taskCount = 0;

            ScheduleShift(view, width, false, _animTasks, ref taskCount);
            ScheduleShift(view, width, true, _animTasks, ref taskCount);

            if (taskCount == 0) 
                return UniTask.CompletedTask;
            
            if (_animTasks.Length != taskCount)
                Array.Resize(ref _animTasks, taskCount);

            return UniTask.WhenAll(_animTasks);
        }

        private void ScheduleShift(IGridView view, int width, bool passIsSpawn, UniTask[] animTasks, ref int taskCount)
        {
            for (int x = 0; x < width; x++)
            {
                ref var state = ref _waveByX[x];

                for (int i = 0; i < _recordCount; i++)
                {
                    ref readonly var fallRecord = ref _records[i];

                    if (!fallRecord.Item) continue;
                        
                    if (fallRecord.FinalCoord.x != x) continue;
                    
                    if (fallRecord.IsSpawn != passIsSpawn) continue;
                    
                    if (_shiftAnimationScheduler.TrySchedule(view, fallRecord, ref state, out var task))
                    {
                        animTasks[taskCount++] = task;
                    }
                }
            }
        }
    }
}