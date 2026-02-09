using System;
using Core.Level;
using Core.SaveSystem;
using UnityEngine;

namespace Core.Models
{
    public sealed class LevelProgressionModel : ILevelProgressionModel
    {
        private const string SaveKey = "level_progression";

        private readonly IJsonSaveService _saveService;
        private readonly ILevelDataReader _levelDataReader;

        private LevelProgressionData _levelProgressionData;
        public int MaxLevel => _levelDataReader.LevelSize;
        public int CurrentLevelIndex => _levelProgressionData.currentLevelIndex;
        public int LevelCompletionCount => _levelProgressionData.levelCompletionCount;
        private bool ResetIndexOnLimit => true; //TODO: Get this form config
        public event Action OnLevelChanged;
        public LevelProgressionModel(IJsonSaveService saveService, ILevelDataReader levelDataReader)
        {
            _saveService = saveService;
            _levelDataReader = levelDataReader;

            var defaultState = new LevelProgressionData
            {
                currentLevelIndex = 0,
                levelCompletionCount = 1
            };
            
            _levelProgressionData = _saveService.LoadFromTextFile(SaveKey, defaultState);
            
            _levelProgressionData.currentLevelIndex = Mathf.Clamp(_levelProgressionData.currentLevelIndex, 0, MaxLevel - 1);
        }

        public void SetLevel(int levelIndex)
        {
            if (MaxLevel <= 0) return;

            var index = Mathf.Clamp(levelIndex, 0, MaxLevel - 1);
            
            if (index == _levelProgressionData.currentLevelIndex) return;

            _levelProgressionData.currentLevelIndex = index;
            
            RaiseChanged();
        }

        public void AdvanceLevel()
        {
            if (MaxLevel <= 0) return;

            var next = _levelProgressionData.currentLevelIndex + 1;
            
            next = next >= MaxLevel ? (ResetIndexOnLimit ? 0 : MaxLevel - 1) : next;
            
            _levelProgressionData.currentLevelIndex = next;
            
            _levelProgressionData.levelCompletionCount++;
            
            RaiseChanged();
        }

        public void ResetProgress()
        {
            _levelProgressionData.currentLevelIndex = 0;
            _levelProgressionData.levelCompletionCount = 1;
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            _saveService.SaveToTextFile(SaveKey, _levelProgressionData);
            OnLevelChanged?.Invoke();
        }
    }
}