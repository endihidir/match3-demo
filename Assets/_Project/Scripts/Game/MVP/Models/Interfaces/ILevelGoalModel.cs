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
        event Action<IDamageableObstacle, Vector3, Vector2> OnGoalCountUpdate;
        public int MoveCount { get; }
        bool IsAllGoalsComplete { get; }
        bool IsAllMovesFinished { get; }
        void Initialize(List<LevelGoal> goals, int moveCount);
        void CountGoal(IDamageableObstacle damageableObstacle, Vector3 worldPos, Vector2 size);
        void DecreaseMoveCount();
        bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal);
    }
}