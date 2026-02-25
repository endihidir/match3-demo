using System;
using Game.Grid.Item;
using Game.HUD.Handlers.Data;
using Game.Views;
using UnityEngine;

namespace Game.HUD.Handlers
{
    public interface IGoalFxHandler
    {
        event Action<GridObjectType> OnGoalFxComplete;
        void PlayFX(GoalSlotView targetSlotView, GoalCollectedData goalCollectedData, Transform fxHolder);
    }
}