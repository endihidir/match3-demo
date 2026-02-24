using System;
using Game.Grid.Item;
using Game.Views;
using UnityEngine;

namespace Game.HUD.Handlers
{
    public interface IGoalFxHandler
    {
        event Action<ObstacleType> OnGoalFxComplete;
        void PlayFX(GoalSlotView targetSlotView, Vector3 worldPos, Vector2 rectSize, Transform fxHolder);
    }
}