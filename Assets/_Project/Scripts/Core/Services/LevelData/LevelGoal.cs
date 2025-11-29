using System;
using Core.Item;
using UnityEngine;

namespace Core.Level
{
    [Serializable]
    public class LevelGoal
    {
        [field: SerializeField] public int Count { get; set; }
        [field: SerializeField] public ObstacleType ObstacleType { get; set; }
        public event EventHandler<int> OnGoalUpdated;
        public void RaiseGoalStatus() => OnGoalUpdated?.Invoke(this, Count);
        public LevelGoal Clone()
        {
            return new LevelGoal
            {
                ObstacleType = ObstacleType,
                Count = Count
            };
        }
    }
}