using System;
using Game.Configs;
using Game.Grid.Item;
using Game.Views;
using Game.View.Factories;
using DG.Tweening;
using UnityEngine;

namespace Game.HUD.Handlers
{
    public sealed class GoalFxHandler : IGoalFxHandler
    {
        private const float FX_INTERVAL = 0.05f;
        private readonly IFXViewFactory _fxFactory;
        private readonly ObstacleConfigContainerSO _obstacleConfigContainer;
        private float _lastFxTime;
        public event Action<ObstacleType> OnGoalFxComplete;

        public GoalFxHandler(IFXViewFactory fxFactory, GridConfigContainerSO gridConfigContainer)
        {
            _fxFactory = fxFactory;
            _obstacleConfigContainer = gridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }

        public void PlayFX(GoalSlotView targetSlotView, Vector3 worldPos, Vector2 rectSize, Transform fxHolder)
        {
            if (!_obstacleConfigContainer.Configs.TryGet(targetSlotView.ObstacleType, out var config)) return;
            
            var goalFxView = PrepareFxView(fxHolder, worldPos, rectSize, config);
            var targetPos = targetSlotView.transform.position;
            var targetSize = targetSlotView.GetIconSize();
            
            var delay = CalculateDelay();
            goalFxView.SizeAnimation.SetRectSize(targetSize, 0.75f, delay);
            goalFxView.MoveAnimation.MoveTo(targetPos, 0.75f, delay, Ease.InBack)
                                    .OnComplete(() => OnFxComplete(goalFxView, targetSlotView.ObstacleType));
        }

        private GoalFxView PrepareFxView(Transform fxHolder, Vector3 worldPos, Vector2 rectSize, ObstacleDataSO config)
        {
            var goalFxView = _fxFactory.GetFX<GoalFxView>();
            var sprite = config.CrackedSprites.Length > 0 ? config.CrackedSprites[0] : config.Icon;
            goalFxView.transform.SetParent(fxHolder, false);
            goalFxView.transform.position = worldPos;
            goalFxView.ImageFxModule.SetSprite(sprite);
            goalFxView.ImageFxModule.SetRectSize(rectSize);
            return goalFxView;
        }

        private void OnFxComplete(GoalFxView fxView, ObstacleType obstacleType)
        {
            _fxFactory.ReleaseFX(fxView);
            OnGoalFxComplete?.Invoke(obstacleType);
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