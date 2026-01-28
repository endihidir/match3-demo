using System;
using System.Collections.Generic;
using Core.Item;
using Core.Level;

namespace Core.Models
{
    public interface ILevelGoalModel
    {
        event Action OnAllGoalsComplete;
        event Action<int> OnMoveCountUpdate;
        event Action<ObstacleType,int> OnGoalCountUpdate;
        bool IsAllGoalsComplete { get; }
        bool IsMoveCountFinished { get; }
        void Initialize(List<LevelGoal> goals, int moveCount);
        void CountGoal(ObstacleType obstacleType, int count);
        void DecreaseMoveCount();
        bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal);
    }
}