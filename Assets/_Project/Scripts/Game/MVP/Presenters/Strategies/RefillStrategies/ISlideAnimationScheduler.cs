using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public interface ISlideAnimationScheduler
    {
        bool TrySchedule(IGridView view, in SlideDownMoveRecord record, Vector2Int[] pathCoord, int[] pathNext, ref ColumnWaveState state, out UniTask task);
    }
}