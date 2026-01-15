using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public interface IShiftAnimationScheduler
    {
        void Schedule(IGridView view, FallDownMoveRecord record, UniTask[] animTasks, ref int taskCount, ref ColumnWaveState state);
    }
}