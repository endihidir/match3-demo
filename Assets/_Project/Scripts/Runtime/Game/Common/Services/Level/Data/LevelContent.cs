using System;
using System.Collections.Generic;
using Game.Grid.Item;

namespace Game.Level.Data
{
    [Serializable]
    public class LevelContent
    {
        public GridObjectType[,] gridObjectTypes;
        public List<LevelGoal> levelGoals;
    }
}