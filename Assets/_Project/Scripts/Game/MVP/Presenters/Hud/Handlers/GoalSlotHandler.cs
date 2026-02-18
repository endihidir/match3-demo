using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Level;
using Core.UI;
using Core.Utils;

namespace Core.Handlers
{
    public class GoalSlotHandler : IGoalSlotHandler
    {
        private readonly ISlotViewFactory _slotViewFactory;
        private readonly ObstacleConfigContainerSO _obstacleConfigContainer;
        public GoalSlotView[] GoalSlotViews { get; private set; }
        private SerializedDictionary<ObstacleType, GoalSlotView> SlotByType { get; set; }

        public GoalSlotHandler(ISlotViewFactory slotViewFactory, GridConfigContainerSO gridConfigContainer)
        {
            _slotViewFactory = slotViewFactory;
            _obstacleConfigContainer = gridConfigContainer.GetConfig<ObstacleConfigContainerSO>();
        }
        
        public void PopulateSlotViews(List<LevelGoal> levelGoals)
        {
            GoalSlotViews = new GoalSlotView[levelGoals.Count];

            for (var i = 0; i < levelGoals.Count; i++)
            {
                var levelGoal = levelGoals[i];
                
                if(!TryCreateSlot(levelGoal.ObstacleType, out var slotView)) continue;
                
                slotView.SetGoalCount(levelGoal.Count);
                
                GoalSlotViews[i] = slotView;
            }
            
            SlotByType = new SerializedDictionary<ObstacleType, GoalSlotView>(GoalSlotViews.ToDictionary(x => x.ObstacleType));
        }
        
        public void ReleaseAllGoalSlots() => _slotViewFactory.ReleaseSlotsByType<GoalSlotView>();
        
        public bool TryGetGoalSlotView(ObstacleType obstacleType, out GoalSlotView goalSlotView)
        {
            if (SlotByType.TryGetValue(obstacleType, out goalSlotView)) return true;
            
            EditorLogger.LogError($"{obstacleType} slot view not found!");
            
            return false;
        }
        
        private bool TryCreateSlot(ObstacleType obstacleType, out GoalSlotView goalSlotView)
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