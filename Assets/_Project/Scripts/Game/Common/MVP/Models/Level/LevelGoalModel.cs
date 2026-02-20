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
        public bool IsAllGoalsComplete => _totalGoalCount <= 0;
        public event Action OnGoalsComplete;
        public event Action OnMoveCountUpdate;
        public event Action<IDamageableGridObject, Vector3, Vector2> OnGoalProgressUpdate;
        
        public void Initialize(List<LevelGoal> goals, int moveCount)
        {
            _goals = goals.Select(g => g.Clone()).ToList();
            _totalGoalCount = _goals.Sum(x => x.Count);
            MoveCount = moveCount;
            
            if (IsAllGoalsComplete) 
                OnGoalsComplete?.Invoke();
        }

        public void ProgressGoal(IDamageableGridObject damagableObj, Vector3 worldPos, Vector2 rectSize)
        {
            if (IsAllGoalsComplete) return;
            
            foreach (var levelGoal in _goals)
            {
                if (levelGoal.ObstacleType != damagableObj.ObstacleType) continue;
                if (levelGoal.Count == 0) continue;

                var before = levelGoal.Count;
                var removed = Mathf.Min(before, 1);

                levelGoal.Count = before - removed;
                _totalGoalCount -= removed;

                OnGoalProgressUpdate?.Invoke(damagableObj, worldPos, rectSize);
                break;
            }

            if (IsAllGoalsComplete)
            {
                OnGoalsComplete?.Invoke();
            }
        }

        public void ConsumeMove()
        {
            if (IsAllMovesFinished) return;
            
            MoveCount = Mathf.Max(0, MoveCount - 1);
            
            OnMoveCountUpdate?.Invoke();
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