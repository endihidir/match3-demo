using System;
using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Level
{
    public class LevelDefinition
    {
        public int LevelNumber { get; private set; }
        public GridObjectTypeData[,] GridObjectTypes { get; private set; }
        public Vector2Int GridSize { get; private set; }
        public List<LevelGoal> Goals { get; private set; }
        public int MoveLimit { get; private set; }

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
            return $"LevelNumber:{LevelNumber}, GridSize:{GridSize}, NumberOfMoves:{MoveLimit}, Goals:{Goals.Count}";
        }
    }
    
    [Serializable]
    public class GridObjectTypeData
    {
        public GridItemKind gridItemKind;
        public int typeId;
    }
}