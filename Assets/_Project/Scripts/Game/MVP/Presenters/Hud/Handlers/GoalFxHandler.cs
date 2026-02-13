using System;
using System.Collections.Generic;
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
        private readonly List<GoalFxData> _goalFxDataList = new();
        public event Action<ObstacleType> OnGoalFxCompleted;

        public GoalFxHandler(IFXViewFactory fxFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _fxFactory = fxFactory;
            _obstacleConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }
        
        public void QueueFX(GoalSlotView targetSlotView, Transform fxHolder, Vector3 startWorldPos, Vector2 startSize)
        {
            if (!_obstacleConfigContainer.Configs.TryGet(targetSlotView.ObstacleType, out var config)) return;
            
            var goalFxView = PrepareFxView(fxHolder, startWorldPos, startSize, config);
            var goalFxData = CreateFxData(goalFxView, targetSlotView);
            _goalFxDataList.Add(goalFxData);
        }
        
        public void PlayQueuedFX()
        {
            for (var i = 0; i < _goalFxDataList.Count; i++)
            {
                var goalFxData = _goalFxDataList[i];
                PlayFX(goalFxData, i * 0.05f);
            }
            
            _goalFxDataList.Clear();
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
        
        private static GoalFxData CreateFxData(GoalFxView goalFxView, GoalSlotView targetSlotView)
        {
            var targetWorldPos = targetSlotView.transform.position;
            var targetSize = targetSlotView.GetIconSize();
            var goalFxData = new GoalFxData(goalFxView, targetSlotView.ObstacleType, targetWorldPos, targetSize);
            return goalFxData;
        }
        
        private void PlayFX(GoalFxData goalFxData, float delay)
        {
            goalFxData.FxView.Activate();
            goalFxData.FxView.SizeAnimation.SetRectSize(goalFxData.TargetSize, .75f, delay);
            goalFxData.FxView.MoveAnimation.MoveTo(goalFxData.TargetWorldPos, .75f, delay, Ease.InBack)
                                              .OnComplete(() => OnFxComplete(goalFxData));
        }
        
        private void OnFxComplete(GoalFxData data)
        {
            _fxFactory.ReleaseFX(data.FxView);
            OnGoalFxCompleted?.Invoke(data.ObstacleType);
        }
    }
}