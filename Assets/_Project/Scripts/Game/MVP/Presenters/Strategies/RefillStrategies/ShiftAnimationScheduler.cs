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

        // Schedules a vertical shift (fall/spawn-fall) using a minimal request.
        public void Schedule(IGridView view, FallDownMoveRecord record, UniTask[] animTasks, ref int taskCount, ref ColumnWaveState state)
        {
            if (!record.Item) return;

            var wave = record.IsSpawn ? state.SpawnFall + state.SpawnSlide + state.Slide + state.Fall : state.Fall;

            if (record.IsSpawn) state.SpawnFall++; else state.Fall++;

            var delay = wave * _animationSettings.ShiftDelayMultiplier;

            var startWorld = record.Item.transform.position;
            
            var finalWorld = view.GridToWorld(record.FinalCoord);
            
            var distCells = Mathf.Abs(finalWorld.y - startWorld.y) / view.GetCellSize();

            var tween = record.Item.ItemAnimation.Shift(finalWorld, distCells, delay);
            
            animTasks[taskCount++] = tween.ToUniTask();
        }
    }
}