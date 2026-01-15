using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public interface ISlideAnimationScheduler
    {
        void Schedule(IGridView view, SlideDownMoveRecord record, Vector2Int[] pathCoord, int[] pathNext, UniTask[] animTasks, ref int taskCount, ref ColumnWaveState state);
    }
}