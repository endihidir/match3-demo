using System.Collections.Generic;
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
        private readonly List<PendingGoalFX> _pendingAnimations = new();

        public GoalFxAnimationHandler(IHudView hudView, IAnimatedFXViewFactory factory)
        {
            _hudView = hudView;
            _factory = factory;
        }
    
        public void QueueAnimation(IDamageableItem item, Vector3 worldPos, Vector2 size)
        {
            var obstacleType = item.ObstacleType;
            
            if (!_hudView.TryGetGoalSlotView(obstacleType, out var slotView)) return;
            
            var sprite = slotView.GetIcon();
            
            var goalFxView = _factory.GetAnimatedFX<GoalFxView>(_hudView.GoalFxHolder, worldPos, sprite, size,false);
            
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
            
            data.FxView.MoveTo(slotView.transform.position, 0.75f, delay, Ease.InBack)
                       .SetSize(slotView.GetIconSize(), 0.75f, delay)
                       .OnMoveComplete(() => OnComplete(data));
        }
    
        private void OnComplete(PendingGoalFX data)
        {
            _factory.ReleaseAnimatedFX(data.FxView);
            _hudView.DecreaseGoalCount(data.ObstacleType);
        }
    }
}