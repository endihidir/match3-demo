using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public partial class FallDownFillStrategy
    {
        private UniTask PlayAnimations(GridStateContext context)
        {
            var recordCount = _recordBuffer.Count;
            if (recordCount == 0)
                return UniTask.CompletedTask;

            var view = context.GridView;
            var width = context.GridModel.Width;

            // Prepare sort order
            _sortOrder.Prepare(recordCount);
            _sortOrder.Sort(recordCount, new FallMoveOrderComparer(_recordBuffer.Records));

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

                if (!record.Item) continue;
                if (record.IsSpawn != passIsSpawn) continue;

                var x = record.FinalCoord.x;
                var startTime = _timeline.GetColumnTime(x);

                if (_fallAnimationScheduler.TrySchedule(view, record, startTime, out var endTime, out var task))
                {
                    _timeline.SetColumnTime(x, endTime);
                    _taskCollector.Add(task);
                }
            }
        }
    }
}
