using System;
using Core.Level;
using Core.MVPContext.Interfaces;
using Core.SaveSystem;
using UnityEngine;

namespace Core.Models
{
    public interface ILevelProgressReadModel : IModel
    {
        event Action OnProgressChanged;
        int CurrentLevelIndex { get; }
        int DisplayLevelNumber { get; }
        int MaxLevel { get; }
    }

    public interface ILevelProgressWriteModel : IModel
    {
        void SetLevel(int levelIndex);
        void AdvanceLevel();
        void ResetProgress();
    }
    
    public sealed class LevelProgressModel : ILevelProgressReadModel, ILevelProgressWriteModel
    {
        private const string SaveKey = "level_progress";

        private readonly IDataPersistenceService _persistence;
        private readonly ILevelDataService _levelDataService;

        private LevelProgressData _levelProgressData;

        public int CurrentLevelIndex => _levelProgressData.currentLevelIndex;
        public int DisplayLevelNumber => _levelDataService.UseInfiniteLevel ? _levelProgressData.displayLevelNumber : _levelProgressData.currentLevelIndex + 1;
        public int MaxLevel => _levelDataService.LevelDefinitions?.Length ?? 0;

        public event Action OnProgressChanged;

        public LevelProgressModel(IDataPersistenceService persistence, ILevelDataService levelDataService)
        {
            _persistence = persistence;
            _levelDataService = levelDataService;

            var defaultState = new LevelProgressData
            {
                currentLevelIndex = 0,
                displayLevelNumber = 1
            };
            
            _levelProgressData = _persistence.LoadFromJson(SaveKey, defaultState);
            
            Clamp();
        }

        private void Clamp()
        {
            var max = MaxLevel;
            
            _levelProgressData.currentLevelIndex = max > 0 ? Mathf.Clamp(_levelProgressData.currentLevelIndex, 0, max - 1) 
                                                           : Mathf.Max(0, _levelProgressData.currentLevelIndex);
            
            if (!_levelDataService.UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = _levelProgressData.currentLevelIndex + 1;
        }

        public void SetLevel(int levelIndex)
        {
            var max = MaxLevel;
            
            if (max <= 0) return;

            var clamped = Mathf.Clamp(levelIndex, 0, max - 1);
            
            if (clamped == _levelProgressData.currentLevelIndex) return;

            _levelProgressData.currentLevelIndex = clamped;

            if (!_levelDataService.UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = clamped + 1;

            RaiseChanged();
        }

        public void AdvanceLevel()
        {
            var max = MaxLevel;
            
            if (max <= 0) return;

            var next = _levelProgressData.currentLevelIndex + 1;

            if (next >= max)
                next = _levelDataService.ResetLevelOnLimit ? 0 : max - 1;

            _levelProgressData.currentLevelIndex = next;

            if (_levelDataService.UseInfiniteLevel)
            {
                _levelProgressData.displayLevelNumber++;
            }
            else
            {
                _levelProgressData.displayLevelNumber = next + 1;
            }

            RaiseChanged();
        }

        public void ResetProgress()
        {
            _levelProgressData.currentLevelIndex = 0;
            _levelProgressData.displayLevelNumber = 1;

            RaiseChanged();
        }

        private void RaiseChanged()
        {
            OnProgressChanged?.Invoke();
            
            _persistence.SaveToJson(SaveKey, _levelProgressData);
        }

        [Serializable]
        private struct LevelProgressData
        {
            public int currentLevelIndex;
            public int displayLevelNumber;
        }
    }
}