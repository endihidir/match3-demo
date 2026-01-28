using System.Collections.Generic;
using Core.Item.Factories;
using Core.Level;
using Core.UI;

namespace Core.Extensions
{
    public static class SlotViewFactoryExtensions
    {
        public static void PopulateSlotViews(this IGoalSlotFactory slotFactory, List<LevelGoal> levelGoals, out GoalSlotView[] slotViews)
        {
            slotViews = new GoalSlotView[levelGoals.Count];

            for (var i = 0; i < levelGoals.Count; i++)
            {
                var levelGoal = levelGoals[i];
                var slotView = slotFactory.GetSlot<GoalSlotView>(levelGoal.ObstacleType);
                slotView.SetGoalCount(levelGoal.Count);
                slotViews[i] = slotView;
            }
        }
    }
}