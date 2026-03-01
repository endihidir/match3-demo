using System;
using Game.Grid.Item;
using Game.HUD.Handlers.Data;

namespace Game.Level.Handlers
{
    public interface ILevelGoalHandler
    {
        event Action<GoalCollectedData> OnGoalCollected;
        void ProgressMove();
        void ProgressGoal(BaseGridObject gridObject);
        void RegisterGoal(GridObjectType goalType, int count);
    }
}