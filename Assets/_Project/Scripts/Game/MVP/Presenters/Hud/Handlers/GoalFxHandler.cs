using System;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.UI;
using DG.Tweening;
using UnityEngine;

namespace Core.Presenters
{
    public sealed class GoalFxHandler : IGoalFxHandler
    {
        private readonly IFXViewFactory _fxFactory;
        private readonly ObstacleConfigContainerSO _obstacleConfigContainer;
        private float _lastFxTime;
        private const float FX_INTERVAL = 0.05f;

        public event Action<ObstacleType> OnGoalFxCompleted;

        public GoalFxHandler(IFXViewFactory fxFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _fxFactory = fxFactory;
            _obstacleConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }

        public void PlayFX(GoalSlotView targetSlotView, Transform fxHolder, Vector3 startWorldPos, Vector2 startSize)
        {
            if (!_obstacleConfigContainer.Configs.TryGet(targetSlotView.ObstacleType, out var config)) return;

            var currentTime = Time.time;
            
            var delay = 0f;
            
            if (currentTime - _lastFxTime < FX_INTERVAL)
            {
                delay = FX_INTERVAL - (currentTime - _lastFxTime);
            }

            _lastFxTime = currentTime + delay;

            PlayFXInternal(targetSlotView, fxHolder, startWorldPos, startSize, config, delay);
        }

        private void PlayFXInternal(GoalSlotView targetSlotView, Transform fxHolder, Vector3 startWorldPos, Vector2 startSize, ObstacleDataSO config, float delay)
        {
            var goalFxView = PrepareFxView(fxHolder, startWorldPos, startSize, config);
            var targetWorldPos = targetSlotView.transform.position;
            var targetSize = targetSlotView.GetIconSize();

            goalFxView.Activate();
            goalFxView.SizeAnimation.SetRectSize(targetSize, 0.75f, delay);
            goalFxView.MoveAnimation.MoveTo(targetWorldPos, 0.75f, delay, Ease.InBack)
                .OnComplete(() => OnFxComplete(goalFxView, targetSlotView.ObstacleType));
        }

        private GoalFxView PrepareFxView(Transform fxHolder, Vector3 startWorldPos, Vector2 startSize, ObstacleDataSO config)
        {
            var goalFxView = _fxFactory.GetFX<GoalFxView>(false);
            var sprite = config.CrackedSprites.Length > 0 ? config.CrackedSprites[0] : config.icon;
            goalFxView.transform.SetParent(fxHolder, false);
            goalFxView.transform.position = startWorldPos;
            goalFxView.ImageFxModule.SetSprite(sprite);
            goalFxView.ImageFxModule.SetSize(startSize);
            return goalFxView;
        }

        private void OnFxComplete(GoalFxView fxView, ObstacleType obstacleType)
        {
            _fxFactory.ReleaseFX(fxView);
            OnGoalFxCompleted?.Invoke(obstacleType);
        }
    }
}