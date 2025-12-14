using System;
using System.Collections.Generic;
using Core.Item;

namespace Core.Level
{
    [Serializable]
    public class LevelContentData
    {
        public GridObjectTypeData[,] gridObjectTypes;
        public List<LevelGoal> levelGoals;
    }
}