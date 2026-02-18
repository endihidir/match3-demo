using System;
using System.Collections.Generic;
using Core.Item;

namespace Core.Level
{
    [Serializable]
    public class LevelContent
    {
        public GridObjectType[,] gridObjectTypes;
        public List<LevelGoal> levelGoals;
    }
}