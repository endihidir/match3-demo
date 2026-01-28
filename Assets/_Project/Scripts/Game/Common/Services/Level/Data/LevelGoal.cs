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