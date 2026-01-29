using Core.Config;
using Core.Configs;
using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public class GoalSlotFactory : IGoalSlotFactory
    {
        private readonly ISlotViewFactory _slotViewFactory;
        private readonly ObstacleConfigContainerSO _obstacleConfigContainer;

        public GoalSlotFactory(ISlotViewFactory slotViewFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _slotViewFactory = slotViewFactory;
            _obstacleConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }
        
        public T GetSlot<T>(ObstacleType obstacleType) where T : GoalSlotView
        {
            var slotView = _slotViewFactory.GetSlot<T>();

            if (!_obstacleConfigContainer.Configs.TryGet(obstacleType, out var obstacleConfig)) return slotView;
            
            slotView.SetCollectible(obstacleConfig.IsCollectible);
            
            slotView.SetType(obstacleType);
            
            slotView.SetIcon(obstacleConfig.icon);

            return slotView;
        }
        
        public void ReleaseSlot(GoalSlotView slot) => _slotViewFactory.ReleaseSlot(slot);
        
        public void ReleaseSlot(Transform slot) => _slotViewFactory.ReleaseSlot(slot);
        
        public void ReleaseSlotsByType<T>() where T : GoalSlotView => _slotViewFactory.ReleaseSlotsByType<T>();
        
        public void RemovePoolsByType<T>() where T : GoalSlotView => _slotViewFactory.RemoveSlotPoolByType<T>();
    }
}