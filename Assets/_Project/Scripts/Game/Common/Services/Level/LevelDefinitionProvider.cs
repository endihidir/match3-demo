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
        GridObjectType[,] GetGridObjectTypes();
    }
    
    public class LevelDefinitionProvider : ILevelDefinitionProvider
    {
        private readonly ILevelDataReader _dataReader;
        private readonly ILevelProgressionReader _progressionReader;

        public LevelDefinitionProvider(ILevelDataReader dataReader, ILevelProgressionReader progressionReader)
        {
            _dataReader = dataReader;
            _progressionReader = progressionReader;
        }

        public int GetMoveCount() => _dataReader.GetLevelDefinition(_progressionReader.CurrentLevelIndex).MoveCount;
        public Vector2Int GetGridSize() => _dataReader.GetLevelDefinition(_progressionReader.CurrentLevelIndex).GridSize;
        public List<LevelGoal> GetLevelGoals() => _dataReader.GetLevelDefinition(_progressionReader.CurrentLevelIndex).Goals;
        public GridObjectType[,] GetGridObjectTypes() => _dataReader.GetLevelDefinition(_progressionReader.CurrentLevelIndex).GridObjectTypes;
        public int GetLevelNumber(bool useInfiniteCount = false) => useInfiniteCount 
                                                                  ? _progressionReader.DisplayLevelNumber
                                                                  : _dataReader.GetLevelDefinition(_progressionReader.CurrentLevelIndex).LevelNumber;
    }
}