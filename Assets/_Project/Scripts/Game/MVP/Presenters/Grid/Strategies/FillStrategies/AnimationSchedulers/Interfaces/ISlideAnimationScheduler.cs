using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Strategies.Schedulers
{
    public interface ISlideAnimationScheduler
    {
        bool TrySchedule(IGridView view, in SlideDownMoveRecord record, Vector2Int[] pathCoord, int[] pathNext, float startTime, out float endTime, out UniTask task);
    }
}