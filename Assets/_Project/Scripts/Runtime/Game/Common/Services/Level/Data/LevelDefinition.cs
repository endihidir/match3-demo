using System.Collections.Generic;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Level.Data
{
    public class LevelDefinition
    {
        public int LevelNumber { get; }
        public GridObjectType[,] GridObjectTypes { get; }
        public Vector2Int GridSize { get; }
        public List<LevelGoal> Goals { get; }
        public int MoveCount { get; }

        public LevelDefinition(int levelNumber, int moveCount, GridObjectType[,] gridItems, List<LevelGoal> goals)
        {
            LevelNumber = levelNumber;
            MoveCount = moveCount;
            GridObjectTypes = gridItems;
            GridSize = new Vector2Int(GridObjectTypes.GetLength(0), GridObjectTypes.GetLength(1));
            Goals = goals;
        }

        public override string ToString() => $"LevelNumber:{LevelNumber}, MoveCount:{MoveCount}, GridSize:{GridSize}, Goals:{Goals.Count}";
    }
}