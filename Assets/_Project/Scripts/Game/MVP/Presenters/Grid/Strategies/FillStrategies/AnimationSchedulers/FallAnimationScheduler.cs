using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Strategies.Schedulers
{
    public sealed class FallAnimationScheduler : IFallAnimationScheduler
    {
        private readonly IGridView _gridView;
        public FallAnimationScheduler(IGridView gridView) => _gridView = gridView;
        
        public bool TrySchedule(in FallDownMoveRecord record, float startTime, out float endTime, out UniTask task)
        {
            task = UniTask.CompletedTask;
            
            endTime = startTime;
            
            if (!record.Item) return false;

            var startWorld = record.Item.transform.position;
            
            var finalWorld = _gridView.GridToWorld(record.FinalCoord);
            
            var distCells = Mathf.Abs(finalWorld.y - startWorld.y) / _gridView.GetCellSize();

            var animation = record.Item.Animation;
            
            var tween = animation.ShiftTo(finalWorld, distCells, startTime);
            
            task = tween?.ToUniTask() ?? UniTask.CompletedTask;
            
            endTime = startTime + animation.GetShiftDelay();

            return true;
        }
    }
}