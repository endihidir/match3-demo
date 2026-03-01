using System;
using System.Collections.Generic;
using Game.Grid.Item;
using Game.Level.Data;

namespace Game.Level.Models
{
    public interface ILevelGoalModel
    {
        event Action OnGoalCountUpdate;
        event Action OnMoveCountUpdate;
        List<LevelGoal> Goals { get; }
        public int MoveCount { get; }
        bool IsAllGoalsComplete { get; }
        bool IsAllMovesFinished { get; }
        void Initialize(List<LevelGoal> goals, int moveCount);
        void ProgressGoal(GridObjectType gridObjectType);
        void ConsumeMove();
        void AddGoal(GridObjectType gridObjectType, int count);
        bool IsGoalComplete(GridObjectType gridObjectType);
        bool TryGetGoal(GridObjectType gridObjectType, out LevelGoal levelGoal);
    }
}