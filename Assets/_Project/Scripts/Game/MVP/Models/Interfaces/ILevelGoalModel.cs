using System;
using Core.Item;
using Core.Level;

namespace Core.Models
{
    public interface ILevelGoalModel
    {
        event Action OnAllGoalsComplete;
        event Action OnMoveCountsFinished;
        bool IsAllGoalsComplete { get; }
        bool IsMoveCountFinished { get; }
        void CountGoal(ObstacleType obstacleType, int count);
        void DecreaseMoveCount();
        bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal);
    }
}