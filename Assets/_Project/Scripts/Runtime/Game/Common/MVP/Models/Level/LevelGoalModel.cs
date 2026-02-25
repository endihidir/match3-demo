using System;
using System.Collections.Generic;
using System.Linq;
using Game.Grid.Item;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Models
{
    public sealed class LevelGoalModel : ILevelGoalModel
    {
        private List<LevelGoal> _goals;
        private int _totalGoalCount;
        public int MoveCount { get; private set; }
        public bool IsAllMovesFinished => MoveCount <= 0;
        public bool IsAllGoalsComplete => _goals.All(x=> x.Count <= 0);
        public event Action OnGoalCountUpdate;
        public event Action OnMoveCountUpdate;
        
        public void Initialize(List<LevelGoal> goals, int moveCount)
        {
            _goals = goals.Select(g => g.Clone()).ToList();
            OnGoalCountUpdate?.Invoke();
            
            MoveCount = moveCount;
            OnMoveCountUpdate?.Invoke();
        }

        public void ProgressGoal(GridObjectType gridObjectType)
        {
            if (IsAllGoalsComplete) return;

            if (TryGetGoal(gridObjectType, out var goal))
            {
                goal.Count = Mathf.Max(0, goal.Count - 1);
            }

            OnGoalCountUpdate?.Invoke();
        }

        public void ConsumeMove()
        {
            if (IsAllMovesFinished) return;
            
            MoveCount = Mathf.Max(0, MoveCount - 1);
            
            OnMoveCountUpdate?.Invoke();
        }
        
        public bool TryGetGoal(GridObjectType gridObjectType, out LevelGoal levelGoal)
        {
            var goal = _goals.FirstOrDefault(x => x.GridObjectType.Equals(gridObjectType));

            if (goal != null)
            {
                levelGoal = goal;
                return true;
            }

            levelGoal = null;
            return false;
        }
    }
}