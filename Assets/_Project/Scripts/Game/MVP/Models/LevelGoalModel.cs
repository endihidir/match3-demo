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
        
        private int _moveCount, _totalGoalCount;
        public bool IsAllMovesFinished => _moveCount <= 0;
        public bool IsAllGoalsComplete => _totalGoalCount <= 0;
        public event Action OnAllGoalsComplete;
        public event Action<int> OnMoveCountUpdate;
        public event Action<ObstacleType, int> OnGoalCountUpdate;

        public void Initialize(List<LevelGoal> goals, int moveCount)
        {
            _goals = new List<LevelGoal>(goals.Count);
            goals.ForEach(goal => _goals.Add(goal.Clone()));
            _moveCount = moveCount;
            _totalGoalCount = _goals.Sum(x => x.Count);
        }

        public void CountGoal(ObstacleType obstacleType, int count)
        {
            if (IsAllGoalsComplete) return;

            foreach (var levelGoal in _goals)
            {
                if (levelGoal.ObstacleType != obstacleType) continue;
                if (levelGoal.Count == 0) continue;

                var before = levelGoal.Count;
                var removed = Mathf.Min(before, count);

                levelGoal.Count = before - removed;
                _totalGoalCount -= removed;

                OnGoalCountUpdate?.Invoke(levelGoal.ObstacleType, levelGoal.Count);

                if (_totalGoalCount > 0) continue;
                
                _totalGoalCount = 0;
                
                OnAllGoalsComplete?.Invoke();
                return;
            }
        }

        public void DecreaseMoveCount()
        {
            if (IsAllMovesFinished) return;
            
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