using System;
using Cysharp.Threading.Tasks;
using Game.Grid.Contexts;
using Game.Grid.Strategies.Data;
using Game.Views;

namespace Game.Grid.Strategies
{
    public partial class FallDownFillStrategy
    {
        private UniTask PlayAnimations(GridStateContext context)
        {
            var view = context.GridView;
            var width = context.GridModel.Width;

            if (_animTasks.Length < _recordCount)
                Array.Resize(ref _animTasks, _recordCount);

            for (int x = 0; x < width; x++)
                _timelineByX[x] = 0f;

            EnsureBuffers(width);

            for (int i = 0; i < _recordCount; i++)
                _order[i] = i;

            Array.Sort(_order, 0, _recordCount, new FallMoveOrderComparer(_records));

            var taskCount = 0;

            ScheduleShiftByTimeline(view, passIsSpawn: false, ref taskCount);
            ScheduleShiftByTimeline(view, passIsSpawn: true, ref taskCount);

            if (taskCount == 0) 
                return UniTask.CompletedTask;
            
            if (_animTasks.Length != taskCount)
                Array.Resize(ref _animTasks, taskCount);

            return UniTask.WhenAll(_animTasks);
        }

        private void ScheduleShiftByTimeline(IGridView view, bool passIsSpawn, ref int taskCount)
        {
            for (int i = 0; i < _recordCount; i++)
            {
                var recordIndex = _order[i];
                ref readonly var fallRecord = ref _records[recordIndex];

                if (!fallRecord.Item) continue;
                if (fallRecord.IsSpawn != passIsSpawn) continue;

                var x = fallRecord.FinalCoord.x;
                var startTime = _timelineByX[x];

                if (_fallAnimationScheduler.TrySchedule(view, fallRecord, startTime, out var endTime, out var task))
                {
                    _timelineByX[x] = endTime;
                    _animTasks[taskCount++] = task;
                }
            }
        }
    }
}