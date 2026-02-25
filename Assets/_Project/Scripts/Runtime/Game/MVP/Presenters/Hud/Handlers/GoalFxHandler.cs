using System;
using Game.Grid.Item;
using Game.Views;
using Game.View.Factories;
using DG.Tweening;
using Game.HUD.Handlers.Data;
using UnityEngine;

namespace Game.HUD.Handlers
{
    public sealed class GoalFxHandler : IGoalFxHandler
    {
        private const float FX_INTERVAL = 0.05f;
        private readonly IFXViewFactory _fxFactory;
        private float _lastFxTime;
        public event Action<GridObjectType> OnGoalFxComplete;
        public GoalFxHandler(IFXViewFactory fxFactory) => _fxFactory = fxFactory;

        public void PlayFX(GoalSlotView targetSlotView, GoalCollectedData goalCollectedData, Transform fxHolder)
        {
            var goalFxView = PrepareFxView(fxHolder, goalCollectedData);
            var targetPos = targetSlotView.transform.position;
            var targetSize = targetSlotView.GetIconSize();
            
            var delay = CalculateDelay();
            goalFxView.SizeAnimation.SetRectSize(targetSize, 0.75f, delay);
            goalFxView.MoveAnimation.MoveTo(targetPos, 0.75f, delay, Ease.InBack)
                                    .OnComplete(() => OnFxComplete(goalFxView, targetSlotView.GridObjectType));
        }

        private GoalFxView PrepareFxView(Transform fxHolder, GoalCollectedData goalCollectedData)
        {
            var goalFxView = _fxFactory.GetFX<GoalFxView>();
            goalFxView.transform.SetParent(fxHolder, false);
            goalFxView.transform.position = goalCollectedData.WorldPos;
            goalFxView.ImageFxModule.SetRectSize(goalCollectedData.RectSize);
            goalFxView.ImageFxModule.SetSprite(goalCollectedData.GridObjectData.GetCollectibleSprite());
            return goalFxView;
        }

        private void OnFxComplete(GoalFxView fxView, GridObjectType gridObjectType)
        {
            _fxFactory.ReleaseFX(fxView);
            OnGoalFxComplete?.Invoke(gridObjectType);
        }

        private float CalculateDelay()
        {
            var currentTime = Time.time;
            var delay = 0f;
            
            if (currentTime - _lastFxTime < FX_INTERVAL) 
                delay = FX_INTERVAL - (currentTime - _lastFxTime);

            _lastFxTime = currentTime + delay;
            return delay;
        }
    }
}