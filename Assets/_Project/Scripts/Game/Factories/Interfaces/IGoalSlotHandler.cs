using System.Collections.Generic;
using Core.Item;
using Core.Level;
using Core.UI;

namespace Core.Handlers
{
    public interface IGoalSlotHandler
    {
        GoalSlotView[] GoalSlotViews { get; }
        void PopulateSlotViews(List<LevelGoal> levelGoals);
        bool TryGetGoalSlotView(ObstacleType obstacleType, out GoalSlotView goalSlotView);
        void ReleaseAllGoalSlots();
    }
}