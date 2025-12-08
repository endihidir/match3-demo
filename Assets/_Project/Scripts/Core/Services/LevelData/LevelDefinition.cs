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
        public int MoveCount { get; }

        public LevelDefinition(int levelNumber, int moveCount, GridObjectTypeData[,] gridItems, List<LevelGoal> goals)
        {
            LevelNumber = levelNumber;
            MoveCount = moveCount;
            GridObjectTypes = gridItems;
            GridSize = new Vector2Int(GridObjectTypes.GetLength(0), GridObjectTypes.GetLength(1));
            Goals = goals;
        }

        public override string ToString()
        {
            return $"LevelNumber:{LevelNumber}, MoveLimit:{MoveCount}, GridSize:{GridSize}, Goals:{Goals.Count}";
        }
    }
    
    [Serializable]
    public class GridObjectTypeData
    {
        public GridItemKind gridItemKind;
        public int typeId;

        public GridObjectTypeData() { }
        public GridObjectTypeData(GridItemKind gridItemKind, int typeId)
        {
            this.gridItemKind = gridItemKind;
            this.typeId = typeId;
        }
    }
}