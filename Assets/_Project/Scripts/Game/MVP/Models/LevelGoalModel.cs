using System;
using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Level;
using UnityEngine;

namespace Core.Models
{
    public class LevelGoalModel : ILevelGoalModel
    {
        private List<LevelGoal> _goals;
        
        private int _moveCount;
        public bool IsAllGoalsComplete => _goals.TrueForAll(x => x.Count <= 0);
        public bool IsMoveCountFinished => _moveCount <= 0;
        public event Action OnAllGoalsComplete;
        public event Action<int> OnMoveCountUpdate;
        public event Action<ObstacleType, int> OnGoalCountUpdate;

        public void Initialize(List<LevelGoal> goals, int moveCount)
        {
            _goals = new List<LevelGoal>(goals.Count);
            goals.ForEach(goal => _goals.Add(goal.Clone()));
            _moveCount = moveCount;
        }

        public void CountGoal(ObstacleType obstacleType, int count)
        {
            if(IsAllGoalsComplete) return;
            
            foreach (var levelGoal in _goals)
            {
                if (levelGoal.ObstacleType != obstacleType) continue;
                
                if (levelGoal.Count == 0) continue;
                
                levelGoal.Count = Mathf.Max(0, levelGoal.Count - count);
                
                OnGoalCountUpdate?.Invoke(levelGoal.ObstacleType, levelGoal.Count);
            }

            if (IsAllGoalsComplete)
            {
                OnAllGoalsComplete?.Invoke();
            }
        }

        public void DecreaseMoveCount()
        {
            if (IsMoveCountFinished) return;
            
            _moveCount = Mathf.Max(0, _moveCount - 1);
            
            OnMoveCountUpdate?.Invoke(_moveCount);
        }
        
        public bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal)
        {
            var goal = _goals.FirstOrDefault(x => x.ObstacleType.Equals(obstacleType));

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