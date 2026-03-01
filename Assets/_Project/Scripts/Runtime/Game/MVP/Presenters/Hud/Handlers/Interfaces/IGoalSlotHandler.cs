using System.Collections.Generic;
using Game.Grid.Item;
using Game.Views;
using Game.Level.Data;

namespace Game.HUD.Handlers
{
    public interface IGoalSlotHandler
    {
        GoalSlotView[] GoalSlotViews { get; }
        void PopulateSlotViews(List<LevelGoal> levelGoals);
        bool TryGetGoalSlotView(GridObjectType gridObjectType, out GoalSlotView goalSlotView, bool showLogs = false);
    }
}