using System.Collections.Generic;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Level;
using Core.UI;

namespace Core.Handlers
{
    public class GoalSlotHandler : IGoalSlotHandler
    {
        private readonly ISlotViewFactory _slotViewFactory;
        private readonly ObstacleConfigContainerSO _obstacleConfigContainer;

        public GoalSlotHandler(ISlotViewFactory slotViewFactory, GameplayConfigContainer gameplayConfigContainer)
        {
            _slotViewFactory = slotViewFactory;
            _obstacleConfigContainer = gameplayConfigContainer.GridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }
        
        public void PopulateSlotViews(List<LevelGoal> levelGoals, out GoalSlotView[] slotViews)
        {
            slotViews = new GoalSlotView[levelGoals.Count];

            for (var i = 0; i < levelGoals.Count; i++)
            {
                var levelGoal = levelGoals[i];
                
                if(!TryGetSlot(levelGoal.ObstacleType, out var slotView)) continue;
                
                slotView.SetGoalCount(levelGoal.Count);
                
                slotViews[i] = slotView;
            }
        }
        public void ReleaseAllGoalSlots() => _slotViewFactory.ReleaseSlotsByType<GoalSlotView>();
        
        private bool TryGetSlot(ObstacleType obstacleType, out GoalSlotView goalSlotView)
        {
            goalSlotView = null;
            
            if (!_obstacleConfigContainer.Configs.TryGet(obstacleType, out var obstacleConfig)) return false;
            
            goalSlotView = _slotViewFactory.GetSlot<GoalSlotView>();
            
            goalSlotView.Initialize(obstacleType);
            
            goalSlotView.ApplyData(obstacleConfig);

            return true;
        }
    }
}