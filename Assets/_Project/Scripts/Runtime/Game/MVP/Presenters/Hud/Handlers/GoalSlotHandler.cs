using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Game.Configs;
using Game.Grid.Item;
using Game.Views;
using Core.Utils;
using Game.View.Factories;
using Game.Level.Data;

namespace Game.HUD.Handlers
{
    public sealed class GoalSlotHandler : IGoalSlotHandler
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