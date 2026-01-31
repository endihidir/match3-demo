using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IFallAnimationScheduler
    {
        bool TrySchedule(IGridView view, in FallDownMoveRecord record, float startTime, out float endTime, out UniTask task);
    }
}