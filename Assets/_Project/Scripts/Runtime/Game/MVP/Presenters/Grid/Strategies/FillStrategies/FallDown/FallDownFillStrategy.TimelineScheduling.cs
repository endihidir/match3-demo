using System;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy
    {
        /// <summary>
        /// Converts all recorded fall moves into DOTween animations, ordered by
        /// column timeline.
        ///
        /// Pass 1 — existing items that fell (non-spawn).
        /// Pass 2 — newly spawned items.
        /// </summary>
        private UniTask ScheduleAnimations()
        {
            if (_recordCount == 0)
                return UniTask.CompletedTask;

            // Build sort order.
            for (int i = 0; i < _recordCount; i++)
                _order[i] = i;

            Array.Sort(_order, 0, _recordCount, new FallMoveOrderComparer(_records));

            // Pre-size task buffer (only grows, never shrinks → no GC per run).
            if (_animTasks.Length < _recordCount)
                Array.Resize(ref _animTasks, _recordCount * 2);

            var taskCount = 0;

            SchedulePass(passSpawn: false, ref taskCount);
            SchedulePass(passSpawn: true, ref taskCount);

            if (taskCount == 0)
                return UniTask.CompletedTask;

            // Slice exactly taskCount tasks for WhenAll.
            var tasks = new UniTask[taskCount];
            Array.Copy(_animTasks, tasks, taskCount);
            return UniTask.WhenAll(tasks);
        }

        private void SchedulePass(bool passSpawn, ref int taskCount)
        {
            var width = _gridModel.Width;

            for (int i = 0; i < _recordCount; i++)
            {
                var idx = _order[i];
                ref readonly var record = ref _records[idx];

                if (!record.Item) continue;
                if (record.IsSpawn != passSpawn) continue;

                var x= record.FinalCoord.x;
                if (x >= width) continue;

                var startTime = _timelineByX[x];

                if (!_fallAnimationScheduler.TrySchedule(record, startTime, out var endTime, out var task))
                    continue;

                _timelineByX[x] = endTime;

                if (taskCount == _animTasks.Length)
                    Array.Resize(ref _animTasks, _animTasks.Length * 2);

                _animTasks[taskCount++] = task;
            }
        }
    }
}