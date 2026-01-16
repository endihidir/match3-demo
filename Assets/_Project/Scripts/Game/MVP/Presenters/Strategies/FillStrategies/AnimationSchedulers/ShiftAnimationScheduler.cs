using Core.Config;
using Core.Configs;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ShiftAnimationScheduler : IShiftAnimationScheduler
    {
        private readonly FillAnimationSettingsSO _animationSettings;

        public ShiftAnimationScheduler(GameplayConfigContainer config)
        {
            _animationSettings = config.FillAnimationSettings;
        }

        public bool TrySchedule(IGridView view, in FallDownMoveRecord record, float startTime, out float endTime, out UniTask task)
        {
            task = UniTask.CompletedTask;
            
            endTime = startTime;
            
            if (!record.Item) return false;

            var startWorld = record.Item.transform.position;
            
            var finalWorld = view.GridToWorld(record.FinalCoord);
            
            var distCells = Mathf.Abs(finalWorld.y - startWorld.y) / view.GetCellSize();
            
            var tween = record.Item.ItemAnimation.Shift(finalWorld, distCells, startTime);
            
            task = tween.ToUniTask();
            
            endTime = startTime + _animationSettings.ShiftDelay;

            return true;
        }
    }
}