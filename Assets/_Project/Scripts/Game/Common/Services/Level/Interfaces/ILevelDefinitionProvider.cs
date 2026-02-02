using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Level
{
    public interface ILevelDefinitionProvider
    {
        int GetLevelNumber(bool useInfiniteCount = false);
        int GetMoveCount();
        Vector2Int GetGridSize();
        List<LevelGoal> GetLevelGoals();
        GridObjectType[,] GetGridObjectTypes();
    }
}