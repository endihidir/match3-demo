using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGoalSlotFactory
    {
        T GetSlot<T>(ObstacleType obstacleType) where T : GoalSlotView;
        void ReleaseSlot(GoalSlotView slot);
        void ReleaseSlot(Transform slot);
        void ReleaseSlotsByType<T>() where T : GoalSlotView;
        void RemovePoolsByType<T>() where T : GoalSlotView;
    }
}