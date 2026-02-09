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
    public sealed class GoalFxHandler : IGoalFxHandler
    {
        private readonly IHudView _hudView;
        private readonly IFXViewFactory _fxFactory;
        private readonly ObstacleConfigContainerSO  _obstacleConfigContainer;
        private readonly List<PendingGoalFX> _pendingGoalFxList = new();

        public GoalFxHandler(IHudView hudView, IFXViewFactory fxFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _hudView = hudView;
            _fxFactory = fxFactory;
            _obstacleConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }
    
        public void QueueFX(ObstacleType obstacleType, Vector3 worldPos, Vector2 size)
        {
            if(!_obstacleConfigContainer.Configs.TryGet(obstacleType, out var obstacleConfig)) return;
            
            var goalFxView = _fxFactory.GetFX<GoalFxView>(false);
            var sprite = obstacleConfig.CrackedSprites.Length > 0 ? obstacleConfig.CrackedSprites[0] : obstacleConfig.icon;
            goalFxView.Initialize(_hudView.GoalFxHolder, worldPos, sprite, size);
            var pendingFoalFx = new PendingGoalFX(obstacleType, goalFxView);
            _pendingGoalFxList.Add(pendingFoalFx);
        }
    
        public void PlayQueuedFX()
        {
            for (var i = 0; i < _pendingGoalFxList.Count; i++)
            {
                var pending = _pendingGoalFxList[i];
                
                PlayFX(pending, i * 0.05f);
            }
            
            _pendingGoalFxList.Clear();
        }
    
        private void PlayFX(PendingGoalFX data, float delay)
        {
            if (!_hudView.TryGetGoalSlotView(data.ObstacleType, out var slotView)) return;
            
            data.FxView.Activate();
            data.FxView.SizeAnimation.SetRectSize(slotView.GetIconSize(), 0.75f, delay);
            data.FxView.MoveAnimation.MoveTo(slotView.transform.position, 0.75f, delay, Ease.InBack).OnComplete(()=> UpdateView(data));
        }
    
        private void UpdateView(PendingGoalFX data)
        {
            _fxFactory.ReleaseFX(data.FxView);
            _hudView.DecreaseGoalCount(data.ObstacleType);
        }
    }
}