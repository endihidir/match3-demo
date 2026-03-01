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
        private readonly GridConfigContainerSO _gridConfigContainer;
        public GoalSlotView[] GoalSlotViews { get; private set; }
        private SerializedDictionary<GridObjectType, GoalSlotView> SlotByType { get; set; }

        public GoalSlotHandler(ISlotViewFactory slotViewFactory, GridConfigContainerSO gridConfigContainer)
        {
            _slotViewFactory = slotViewFactory;
            _gridConfigContainer = gridConfigContainer;
        }
        
        public void PopulateSlotViews(List<LevelGoal> levelGoals)
        {
            ResetSlotViews();
            
            GoalSlotViews = new GoalSlotView[levelGoals.Count];

            for (var i = 0; i < levelGoals.Count; i++)
            {
                var levelGoal = levelGoals[i];
                
                if(!TryCreateSlot(levelGoal.GridObjectType, out var slotView)) continue;
                
                slotView.SetGoalCount(levelGoal.Count);
                
                GoalSlotViews[i] = slotView;
            }
            
            SlotByType = new SerializedDictionary<GridObjectType, GoalSlotView>(GoalSlotViews.ToDictionary(x => x.GridObjectType));
        }
        
        private void ResetSlotViews()
        {
            if (GoalSlotViews == null) return;
            
            foreach (var slot in GoalSlotViews)
                _slotViewFactory.ReleaseSlot(slot);
        
            GoalSlotViews = null;
            SlotByType = null;
        }
        
        public bool TryGetGoalSlotView(GridObjectType gridObjectType, out GoalSlotView goalSlotView, bool showLogs = false)
        {
            if (SlotByType.TryGetValue(gridObjectType, out goalSlotView)) return true;
            
            if(showLogs)
                EditorLogger.LogError($"{gridObjectType} slot view not found!");
            
            return false;
        }
        
        private bool TryCreateSlot(GridObjectType gridObjectType, out GoalSlotView goalSlotView)
        {
            goalSlotView = null;

            var data = _gridConfigContainer.GetConfigData(gridObjectType);
            
            if (!data) return false;
            
            goalSlotView = _slotViewFactory.GetSlot<GoalSlotView>();
            
            goalSlotView.Initialize(gridObjectType);
            
            goalSlotView.ApplyData(data);

            return true;
        }
    }
}