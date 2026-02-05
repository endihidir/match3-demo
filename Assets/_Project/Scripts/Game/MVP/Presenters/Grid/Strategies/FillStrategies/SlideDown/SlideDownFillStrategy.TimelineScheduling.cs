using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public partial class SlideDownFillStrategy
    {
        private UniTask PlayAnimations(GridStateContext context)
        {
            var recordCount = _recordBuffer.Count;
            if (recordCount == 0)
                return UniTask.CompletedTask;

            var view = context.GridView;
            var width = context.GridModel.Width;

            // Mark IsSlide for each record
            for (int i = 0; i < recordCount; i++)
            {
                ref var record = ref _recordBuffer.GetRef(i);
                if (record.Item && record.PathCount > 0)
                    record.MarkAsSlide(ref _pathPool);
            }

            // Prepare sort order
            _sortOrder.Prepare(recordCount);
            _sortOrder.Sort(recordCount, new SlideMoveOrderComparer(_recordBuffer.Records));

            // Reset timeline
            _timeline.Reset(width);
            _taskCollector.Reset();

            // Schedule non-spawn first, then spawn
            SchedulePass(view, passIsSpawn: false);
            SchedulePass(view, passIsSpawn: true);

            return _taskCollector.WhenAll();
        }

        private void SchedulePass(IGridView view, bool passIsSpawn)
        {
            var recordCount = _recordBuffer.Count;
            var order = _sortOrder.Order;

            for (int i = 0; i < recordCount; i++)
            {
                var recordIndex = order[i];
                ref readonly var record = ref _recordBuffer.GetReadonly(recordIndex);

                if (!record.Item || record.PathCount == 0) continue;
                if (record.IsSpawn != passIsSpawn) continue;

                // Collect all columns used by this move
                CollectUsedColumns(in record);
                var startTime = _timeline.GetStartTime();

                if (!record.IsSlide)
                {
                    // Simple fall
                    var fallRecord = new FallDownMoveRecord(record.Item, record.FinalCoord, record.IsSpawn);

                    if (_fallAnimationScheduler.TrySchedule(view, fallRecord, startTime, out var endTime, out var task))
                    {
                        _timeline.UpdateEndTime(endTime);
                        _taskCollector.Add(task);
                    }
                }
                else
                {
                    // Diagonal slide
                    if (_slideAnimationScheduler.TrySchedule(view, record, ref _pathPool, startTime, out var endTime, out var task))
                    {
                        _timeline.UpdateEndTime(endTime);
                        _taskCollector.Add(task);
                    }
                }
            }
        }

        private void CollectUsedColumns(in SlideDownMoveRecord record)
        {
            _timeline.BeginCollect();

            // Final position
            _timeline.MarkColumn(record.FinalCoord.x);

            // All path nodes
            if (record.IsSlide)
            {
                var node = record.HeadNode;
                while (node >= 0)
                {
                    _timeline.MarkColumn(_pathPool.GetCoord(node).x);
                    node = _pathPool.GetNext(node);
                }
            }
        }
    }
}