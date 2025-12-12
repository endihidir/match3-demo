using System;
using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Level;
using UnityEngine;

namespace Core.Models
{
    public interface ILevelGoalModel
    {
        event Action OnAllGoalsComplete;
        event Action OnMoveCountsFinished;
        bool IsAllGoalsComplete { get; }
        bool IsMoveCountFinished { get; }
        void CountGoal(ObstacleType obstacleType, int count);
        void DecreaseMoveCount();
        bool TryGetGoal(ObstacleType obstacleType, out LevelGoal levelGoal);
    }
    
    public class LevelGoalModel : ILevelGoalModel
    {
        private List<LevelGoal> _goals;
        private int _moveCount;
        private bool _wasAllGoalsComplete, _wasMoveCountFinished;
        
        public event Action OnAllGoalsComplete;
        public event Action OnMoveCountsFinished;
        public bool IsAllGoalsComplete => _goals.TrueForAll(x => x.Count <= 0);
        public bool IsMoveCountFinished => _moveCount <= 0;

        public LevelGoalModel(ILevelProgressReadModel levelProgressReadModel)
        {
            var levelDefinition = levelProgressReadModel.GetLevelDefinition();
            _goals = new List<LevelGoal>(levelDefinition.Goals.Count);
            levelDefinition.Goals.ForEach(goal => _goals.Add(goal.Clone()));

            _moveCount = levelDefinition.MoveCount;
            
            _wasAllGoalsComplete = false;
            _wasMoveCountFinished = false;
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

        public void CountGoal(ObstacleType obstacleType, int count)
        {
            foreach (var levelGoal in _goals)
            {
                if (levelGoal.ObstacleType != obstacleType) continue;
                
                if (levelGoal.Count == 0) continue;
                
                levelGoal.Count = Mathf.Max(0, levelGoal.Count - count);
                
                levelGoal.RaiseGoalStatus();
            }

            if (IsAllGoalsComplete && !_wasAllGoalsComplete)
            {
                _wasMoveCountFinished = true;
                OnAllGoalsComplete?.Invoke();
            }
        }

        public void DecreaseMoveCount()
        {
            _moveCount = Mathf.Max(0, _moveCount - 1);

            if (IsMoveCountFinished && !_wasMoveCountFinished)
            {
                _wasMoveCountFinished = true;
                OnMoveCountsFinished?.Invoke();
            }
        }
    }
}