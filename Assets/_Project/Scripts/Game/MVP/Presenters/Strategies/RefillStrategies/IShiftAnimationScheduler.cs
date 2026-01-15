using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IShiftAnimationScheduler
    {
        bool TrySchedule(IGridView view, FallDownMoveRecord record, ref ColumnWaveState state, out UniTask task);
    }
}