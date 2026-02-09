using System;
using Core.Item;
using Core.UI;
using UnityEngine;

namespace Core.Presenters
{
    public interface IGoalFxHandler
    {
        event Action<ObstacleType> OnGoalFxCompleted;
        void QueueFX(GoalSlotView targetSlotView, Transform fxHolder, Vector3 startWorldPos, Vector2 startSize);
        void PlayQueuedFX();
    }
}