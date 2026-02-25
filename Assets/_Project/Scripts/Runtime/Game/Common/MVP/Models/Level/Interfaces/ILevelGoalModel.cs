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
        public int MoveCount { get; }
        bool IsAllGoalsComplete { get; }
        bool IsAllMovesFinished { get; }
        void Initialize(List<LevelGoal> goals, int moveCount);
        void ProgressGoal(GridObjectType gridObjectType);
        void ConsumeMove();
        bool TryGetGoal(GridObjectType gridObjectType, out LevelGoal levelGoal);
    }
}