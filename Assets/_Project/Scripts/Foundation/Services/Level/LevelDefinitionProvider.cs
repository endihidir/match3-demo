using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Level
{
    public class LevelDefinitionProvider : ILevelDefinitionProvider
    {
        private readonly ILevelDataService _levelDataService;
        private readonly ILevelProgressionModel _progressionModel;

        public LevelDefinitionProvider(ILevelDataService levelDataService, ILevelProgressionModel progressionModel)
        {
            _levelDataService = levelDataService;
            _progressionModel = progressionModel;
        }

        public int GetMoveCount() => _levelDataService.GetLevelDefinition(_progressionModel.CurrentLevelIndex).MoveCount;
        public Vector2Int GetGridSize() => _levelDataService.GetLevelDefinition(_progressionModel.CurrentLevelIndex).GridSize;
        public List<LevelGoal> GetLevelGoals() => _levelDataService.GetLevelDefinition(_progressionModel.CurrentLevelIndex).Goals;
        public GridObjectType[,] GetGridObjectTypes() => _levelDataService.GetLevelDefinition(_progressionModel.CurrentLevelIndex).GridObjectTypes;
        public int GetLevelNumber(bool useLevelCompletionCount = false) => useLevelCompletionCount 
                                                                  ? _progressionModel.LevelCompletionCount + 1
                                                                  : _levelDataService.GetLevelDefinition(_progressionModel.CurrentLevelIndex).LevelNumber;
    }
}