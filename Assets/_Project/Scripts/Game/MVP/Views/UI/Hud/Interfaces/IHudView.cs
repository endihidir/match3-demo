using System;

namespace Core.UI
{
    public interface IHudView
    {
        GoalSlotView[] GoalSlotViews { get; }
        void Initialize(GoalSlotView[] goalSlotViews, int moveCount);
        void SetMoveCount(int moveCount);
    }
}