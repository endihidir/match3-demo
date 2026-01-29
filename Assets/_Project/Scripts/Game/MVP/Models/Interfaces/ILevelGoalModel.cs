using System;
using System.Collections.Generic;
using Core.Item;
using Core.Level;
using UnityEngine;

namespace Core.Models
{
    public interface ILevelGoalModel
    {
        event Action OnAllGoalsComplete;
        event Action OnMoveCountUpdate;
        event Action<ObstacleType, Vector3, Vector2> OnGoalCountUpdate;
        event Action OnGoalCountUpdateComplete;
        public int MoveCount { get; }
        bool IsAllGoalsComplete { get; }
        bool IsAllMovesFinished { get; }
        void Initialize(List<LevelGoal> goals, int moveCount);
        void CountGoal(ObstacleType obstacleType, Vector3 objPos, Vector2 uiSizeDelta);
        void DecreaseMoveCount();
        bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal);
        void RaiseGoalCountUpdateComplete();
    }
}