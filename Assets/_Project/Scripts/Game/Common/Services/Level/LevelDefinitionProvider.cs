using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Level
{
    public class LevelDefinitionProvider : ILevelDefinitionProvider
    {
        private readonly ILevelDataReader _dataReader;
        private readonly ILevelProgressionModel _progressionModel;

        public LevelDefinitionProvider(ILevelDataReader dataReader, ILevelProgressionModel progressionModel)
        {
            _dataReader = dataReader;
            _progressionModel = progressionModel;
        }

        public int GetMoveCount() => _dataReader.GetLevelDefinition(_progressionModel.CurrentLevelIndex).MoveCount;
        public Vector2Int GetGridSize() => _dataReader.GetLevelDefinition(_progressionModel.CurrentLevelIndex).GridSize;
        public List<LevelGoal> GetLevelGoals() => _dataReader.GetLevelDefinition(_progressionModel.CurrentLevelIndex).Goals;
        public GridObjectType[,] GetGridObjectTypes() => _dataReader.GetLevelDefinition(_progressionModel.CurrentLevelIndex).GridObjectTypes;
        public int GetLevelNumber(bool useInfiniteCount = false) => useInfiniteCount 
                                                                  ? _progressionModel.DisplayLevelNumber
                                                                  : _dataReader.GetLevelDefinition(_progressionModel.CurrentLevelIndex).LevelNumber;
    }
}