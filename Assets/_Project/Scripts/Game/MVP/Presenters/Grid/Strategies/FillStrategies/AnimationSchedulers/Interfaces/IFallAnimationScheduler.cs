using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;
using Game.Views;

namespace Game.Grid.Strategies.Schedulers
{
    public interface IFallAnimationScheduler
    {
        bool TrySchedule(IGridView view, in FallDownMoveRecord record, float startTime, out float endTime, out UniTask task);
    }
}