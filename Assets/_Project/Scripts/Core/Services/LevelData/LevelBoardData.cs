using System;
using System.Collections.Generic;

namespace Core.Level
{
    [Serializable]
    public class LevelContentData
    {
        public GridObjectTypeData[,] gridObjectTypes;
        public List<LevelGoal> levelGoals;
    }
}