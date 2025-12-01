using System;
using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Level
{
    public class LevelDefinition
    {
        public int LevelNumber { get; }
        public GridObjectTypeData[,] GridObjectTypes { get; }
        public Vector2Int GridSize { get; }
        public List<LevelGoal> Goals { get; }
        public int MoveLimit { get; }

        public LevelDefinition(int levelNumber, int moveLimit, GridObjectTypeData[,] gridItems, List<LevelGoal> goals)
        {
            LevelNumber = levelNumber;
            MoveLimit = moveLimit;
            GridObjectTypes = gridItems;
            GridSize = new Vector2Int(GridObjectTypes.GetLength(0), GridObjectTypes.GetLength(1));
            Goals = goals;
        }

        public override string ToString()
        {
            return $"LevelNumber:{LevelNumber}, MoveLimit:{MoveLimit}, GridSize:{GridSize}, Goals:{Goals.Count}";
        }
    }
    
    [Serializable]
    public class GridObjectTypeData
    {
        public GridItemKind gridItemKind;
        public int typeId;
    }
}