using System;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Level.Data
{
    [Serializable]
    public class LevelGoal
    {
        [field: SerializeField] public int Count { get; set; }
        [field: SerializeField] public GridObjectType GridObjectType { get; set; }
        public LevelGoal Clone()
        {
            return new LevelGoal
            {
                GridObjectType = GridObjectType,
                Count = Count
            };
        }
    }
}