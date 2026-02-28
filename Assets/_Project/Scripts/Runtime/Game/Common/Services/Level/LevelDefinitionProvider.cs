using System.Collections.Generic;
using Game.Grid.Item;
using Game.Level.Data;
using Game.Level.Models;
using UnityEngine;

namespace Game.Level.Services
{
    public sealed class LevelDefinitionProvider : ILevelDefinitionProvider
    {
        private readonly ILevelDataService _levelDataService;
        private readonly ILevelProgressionModel _progressionModel;
        
        private int CurrentLevelIndex => !_levelDataService.UseTestLevel ? _progressionModel.CurrentLevelIndex : 
                                                                                   _levelDataService.TestLevelIndex;

        public LevelDefinitionProvider(ILevelDataService levelDataService, ILevelProgressionModel progressionModel)
        {
            _levelDataService = levelDataService;
            _progressionModel = progressionModel;
        }

        public int GetMoveCount() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).MoveCount;
        public Vector2Int GetGridSize() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).GridSize;
        public List<LevelGoal> GetLevelGoals() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).Goals;
        public GridObjectType[,] GetGridObjectTypes() => _levelDataService.GetLevelDefinition(CurrentLevelIndex).GridObjectTypes;
        public int GetLevelNumber(bool useLevelCompletionCount = false) => useLevelCompletionCount 
                                                                  ? _progressionModel.LevelCompletionCount + 1
                                                                  : _levelDataService.GetLevelDefinition(CurrentLevelIndex).LevelNumber;
    }
}