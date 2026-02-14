using System;
using Core.Item;
using Core.UI;
using UnityEngine;

namespace Core.Handlers
{
    public interface IGoalFxHandler
    {
        event Action<ObstacleType> OnGoalFxComplete;
        void PlayFX(GoalSlotView targetSlotView, Vector3 startWorldPos, Vector2 rectSize, Transform fxHolder);
    }
}