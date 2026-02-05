using System;
using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        // =========================================================
        // Animation emit (column timeline scheduling)
        // =========================================================

        private UniTask PlayAnimations(GridStateContext context)
        {
            if (_recordCount == 0)
                return UniTask.CompletedTask;

            EnsureOrderCapacity(_recordCount);

            // Update per-record flags (IsSlide) and build sort order.
            for (int i = 0; i < _recordCount; i++)
                _order[i] = i;

            for (int i = 0; i < _recordCount; i++)
            {
                ref var record = ref _records[i];
                if (!record.Item || record.PathCount == 0) continue;

                var startCoord = _pathCoord[record.HeadNode];
                record.IsSlide = startCoord.x != record.FinalCoord.x;
            }

            Array.Sort(_order, 0, _recordCount, new SlideMoveOrderComparer(_records));

            if (_animTasks.Length < _recordCount)
                Array.Resize(ref _animTasks, _recordCount);

            // Reset timelines.
            var width = context.GridModel.Width;
            for (int x = 0; x < width; x++)
                _timelineByX[x] = 0f;

            var taskCount = 0;

            var view = context.GridView;
            ScheduleByTimeline(view, width, passIsSpawn: false, ref taskCount);
            ScheduleByTimeline(view, width, passIsSpawn: true, ref taskCount);

            if (taskCount == 0)
                return UniTask.CompletedTask;

            if (_animTasks.Length != taskCount)
                Array.Resize(ref _animTasks, taskCount);

            return UniTask.WhenAll(_animTasks);
        }

        private void EnsureOrderCapacity(int need)
        {
            if (_order.Length < need)
                Array.Resize(ref _order, need);
        }

        private void ScheduleByTimeline(IGridView view, int width, bool passIsSpawn, ref int taskCount)
        {
            for (int i = 0; i < _recordCount; i++)
            {
                var recordIndex = _order[i];
                ref readonly var record = ref _records[recordIndex];

                if (!record.Item || record.PathCount == 0) continue;
                if (record.IsSpawn != passIsSpawn) continue;

                var usedCount = CollectUsedColumns(width, in record);
                var startTime = GetStartTime(usedCount);

                if (!record.IsSlide)
                {
                    var fallRecord = new FallDownMoveRecord(record.Item, record.FinalCoord, record.IsSpawn);

                    if (_fallAnimationScheduler.TrySchedule(view, fallRecord, startTime, out var endTime, out var task))
                    {
                        UpdateTimeline(usedCount, endTime);
                        _animTasks[taskCount++] = task;
                    }
                }
                else
                {
                    if (_slideAnimationScheduler.TrySchedule(view, record, _pathCoord, _pathNext, startTime, out var endTime, out var task))
                    {
                        UpdateTimeline(usedCount, endTime);
                        _animTasks[taskCount++] = task;
                    }
                }
            }
        }

        private int CollectUsedColumns(int width, in SlideDownMoveRecord record)
        {
            // Reuse _usedColumnsByRecord as a dense list of used X values.
            if (_usedColumnsByRecord.Length < width)
                Array.Resize(ref _usedColumnsByRecord, width);

            _usedColumnStampId++;
            
            if (_usedColumnStampId == int.MaxValue)
            {
                Array.Clear(_usedColumnStamp, 0, _usedColumnStamp.Length);
                _usedColumnStampId = 1;
            }

            var count = 0;

            MarkX(record.FinalCoord.x);

            if (record.IsSlide)
            {
                var node = record.HeadNode;
                while (node >= 0)
                {
                    MarkX(_pathCoord[node].x);
                    node = _pathNext[node];
                }
            }

            return count;

            void MarkX(int x)
            {
                if ((uint)x >= (uint)width) return;
                if (_usedColumnStamp[x] == _usedColumnStampId) return;
                _usedColumnStamp[x] = _usedColumnStampId;
                _usedColumnsByRecord[count++] = x;
            }
        }

        private float GetStartTime(int usedCount)
        {
            var start = 0f;
            for (int i = 0; i < usedCount; i++)
            {
                var x = _usedColumnsByRecord[i];
                var t = _timelineByX[x];
                if (t > start) start = t;
            }

            return start;
        }

        private void UpdateTimeline(int usedCount, float endTime)
        {
            for (int i = 0; i < usedCount; i++)
            {
                var x = _usedColumnsByRecord[i];
                _timelineByX[x] = endTime;
            }
        }

    }
}