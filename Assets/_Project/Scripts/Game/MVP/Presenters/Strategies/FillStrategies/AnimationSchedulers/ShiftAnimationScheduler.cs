using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ShiftAnimationScheduler : IShiftAnimationScheduler
    {
        public bool TrySchedule(IGridView view, in FallDownMoveRecord record, float startTime, out float endTime, out UniTask task)
        {
            task = UniTask.CompletedTask;
            
            endTime = startTime;
            
            if (!record.Item) return false;

            var startWorld = record.Item.transform.position;
            
            var finalWorld = view.GridToWorld(record.FinalCoord);
            
            var distCells = Mathf.Abs(finalWorld.y - startWorld.y) / view.GetCellSize();

            var animation = record.Item.ItemAnimation;
            
            var tween = animation.ShiftTo(finalWorld, distCells, startTime);
            
            task = tween.ToUniTask();
            
            endTime = startTime + animation.GetShiftTimelineDuration(distCells);

            return true;
        }
    }
}