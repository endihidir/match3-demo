using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IShiftAnimationScheduler
    {
        bool TrySchedule(IGridView view, in FallDownMoveRecord record, float startTime, out float endTime, out UniTask task);
    }
}