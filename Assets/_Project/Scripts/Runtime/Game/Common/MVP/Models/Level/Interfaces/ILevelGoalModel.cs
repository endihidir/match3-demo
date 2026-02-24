using System;
using System.Collections.Generic;
using Game.Grid.Item;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Models
{
    public interface ILevelGoalModel
    {
        event Action OnGoalsComplete;
        event Action OnMoveCountUpdate;
        event Action<IDamageableGridObject, Vector3, Vector2> OnGoalProgressUpdate;
        public int MoveCount { get; }
        bool IsAllGoalsComplete { get; }
        bool IsAllMovesFinished { get; }
        void Initialize(List<LevelGoal> goals, int moveCount);
        void ProgressGoal(IDamageableGridObject damagableObj, Vector3 worldPos, Vector2 rectSize);
        void ConsumeMove();
        bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal);
    }
}