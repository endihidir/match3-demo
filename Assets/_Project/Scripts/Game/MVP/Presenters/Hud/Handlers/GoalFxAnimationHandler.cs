using System.Collections.Generic;
using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.UI;
using DG.Tweening;
using UnityEngine;

namespace Core.Presenters
{
    public sealed class GoalFxAnimationHandler : IGoalFxAnimationHandler
    {
        private readonly IHudView _hudView;
        private readonly IAnimatedFXViewFactory _factory;
        private readonly ObstacleConfigContainerSO  _obstacleConfigContainer;
        private readonly List<PendingGoalFX> _pendingAnimations = new();

        public GoalFxAnimationHandler(IHudView hudView, IAnimatedFXViewFactory factory, GameplayConfigContainer gameplayConfigContainer)
        {
            _hudView = hudView;
            _factory = factory;
            _obstacleConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }
    
        public void QueueAnimation(IDamageableObstacle obstacle, Vector3 worldPos, Vector2 size)
        {
            var obstacleType = obstacle.ObstacleType;
            
            if(!_obstacleConfigContainer.Configs.TryGet(obstacleType, out var obstacleConfig)) return;

            var sprite = obstacleConfig.CrackedSprites.Length > 0 ? obstacleConfig.CrackedSprites[0] : obstacleConfig.icon;
            
            var goalFxView = _factory.GetImageFX<GoalFxView>(_hudView.GoalFxHolder, worldPos, sprite, size,false);
            
            _pendingAnimations.Add(new PendingGoalFX(obstacleType, goalFxView));
        }
    
        public void PlayQueuedAnimations()
        {
            for (var i = 0; i < _pendingAnimations.Count; i++)
            {
                var pending = _pendingAnimations[i];
                
                PlayAnimation(pending, i * 0.05f);
            }
            
            _pendingAnimations.Clear();
        }
    
        private void PlayAnimation(PendingGoalFX data, float delay)
        {
            if (!_hudView.TryGetGoalSlotView(data.ObstacleType, out var slotView)) return;
            
            data.FxView.Activate();

            data.FxView.SizeAnimation.SetRectSize(slotView.GetIconSize(), 0.75f, delay);
            
            data.FxView.MoveAnimation.MoveTo(slotView.transform.position, 0.75f, delay, Ease.InBack).OnComplete(()=> OnComplete(data));
        }
    
        private void OnComplete(PendingGoalFX data)
        {
            _factory.ReleaseImageFX(data.FxView);
            _hudView.DecreaseGoalCount(data.ObstacleType);
        }
    }
}