using System.Collections.Generic;
using Core.Models;
using UnityEngine;

namespace Core.Level
{
    public interface ICurrentLevelProvider
    {
        int GetMoveCount();
        Vector2Int GetGridSize();
        List<LevelGoal> GetLevelGoals();
        GridObjectTypeData[,] GetGridObjectTypes();
    }
    
    public class CurrentLevelProvider : ICurrentLevelProvider
    {
        private readonly ILevelDataReader _levelDataReader;
        private readonly ILevelProgressReader _levelProgressReader;

        public CurrentLevelProvider(ILevelDataReader levelDataReader, ILevelProgressReader levelProgressReader)
        {
            _levelDataReader = levelDataReader;
            _levelProgressReader = levelProgressReader;
        }

        public int GetMoveCount() => _levelDataReader.GetMoveCount(_levelProgressReader.CurrentLevelIndex);
        public Vector2Int GetGridSize() => _levelDataReader.GetGridSize(_levelProgressReader.CurrentLevelIndex);
        public List<LevelGoal> GetLevelGoals() => _levelDataReader.GetLevelGoals(_levelProgressReader.CurrentLevelIndex);
        public GridObjectTypeData[,] GetGridObjectTypes() => _levelDataReader.GetGridObjectTypes(_levelProgressReader.CurrentLevelIndex);
    }
}