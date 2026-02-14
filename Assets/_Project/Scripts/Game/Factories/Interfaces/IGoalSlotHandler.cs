using System.Collections.Generic;
using Core.Level;
using Core.UI;
using UnityEngine;

namespace Core.Handlers
{
    public interface IGoalSlotHandler
    {
        void PopulateSlotViews(List<LevelGoal> levelGoals, out GoalSlotView[] slotViews);
        void ReleaseAllGoalSlots();
    }
}