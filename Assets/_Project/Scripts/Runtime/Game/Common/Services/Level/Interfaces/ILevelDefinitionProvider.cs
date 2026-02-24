using System.Collections.Generic;
using Game.Grid.Item;
using Game.Level.Data;
using UnityEngine;

namespace Game.Level.Services
{
    public interface ILevelDefinitionProvider
    {
        int GetLevelNumber(bool useLevelCompletionCount = false);
        int GetMoveCount();
        Vector2Int GetGridSize();
        List<LevelGoal> GetLevelGoals();
        GridObjectType[,] GetGridObjectTypes();
    }
}