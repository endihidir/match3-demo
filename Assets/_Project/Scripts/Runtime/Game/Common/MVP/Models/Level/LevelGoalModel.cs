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
        public List<LevelGoal> Goals { get; private set; }
        private int _totalGoalCount;
        public int MoveCount { get; private set; }
        public bool IsAllMovesFinished => MoveCount <= 0;
        public bool IsAllGoalsComplete => Goals.All(x=> x.Count <= 0);
        public event Action OnGoalCountUpdate;
        public event Action OnMoveCountUpdate;
        
        public void Initialize(List<LevelGoal> goals, int moveCount)
        {
            Goals = goals.Select(g => g.Clone()).ToList();
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
        
        public void AddNewGoal(GridObjectType gridObjectType, int count)
        {
            if (TryGetGoal(gridObjectType, out var goal))
            {
                goal.Count += count;
            }
            else
            {
                var newGoal = new LevelGoal
                {
                    GridObjectType = gridObjectType,
                    Count = count
                };
                
                Goals.Add(newGoal);
            }
            
            OnGoalCountUpdate?.Invoke();
        }
        
        public bool TryGetGoal(GridObjectType gridObjectType, out LevelGoal levelGoal)
        {
            var goal = Goals.FirstOrDefault(x => x.GridObjectType.Equals(gridObjectType));

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