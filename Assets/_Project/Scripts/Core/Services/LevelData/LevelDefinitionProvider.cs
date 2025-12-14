using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Level
{
    public interface ILevelDefinitionProvider
    {
        int GetLevelNumber(bool useInfiniteCount = false);
        int GetMoveCount();
        Vector2Int GetGridSize();
        List<LevelGoal> GetLevelGoals();
        GridObjectTypeData[,] GetGridObjectTypes();
    }
    
    public class LevelDefinitionProvider : ILevelDefinitionProvider
    {
        private readonly ILevelDataReader _dataReader;
        private readonly ILevelProgressReader _progressReader;

        public LevelDefinitionProvider(ILevelDataReader dataReader, ILevelProgressReader progressReader)
        {
            _dataReader = dataReader;
            _progressReader = progressReader;
        }

        public int GetMoveCount() => _dataReader.GetLevelDefinition(_progressReader.CurrentLevelIndex).MoveCount;
        public Vector2Int GetGridSize() => _dataReader.GetLevelDefinition(_progressReader.CurrentLevelIndex).GridSize;
        public List<LevelGoal> GetLevelGoals() => _dataReader.GetLevelDefinition(_progressReader.CurrentLevelIndex).Goals;
        public GridObjectTypeData[,] GetGridObjectTypes() => _dataReader.GetLevelDefinition(_progressReader.CurrentLevelIndex).GridObjectTypes;
        public int GetLevelNumber(bool useInfiniteCount = false) => useInfiniteCount 
                                                                  ? _progressReader.DisplayLevelNumber
                                                                  : _dataReader.GetLevelDefinition(_progressReader.CurrentLevelIndex).LevelNumber;
    }
}