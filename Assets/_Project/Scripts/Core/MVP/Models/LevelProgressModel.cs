using System;
using Core.Level;
using Core.SaveSystem;
using UnityEngine;

namespace Core.Models
{
    public interface ILevelProgressReadModel
    {
        event Action OnProgressChanged;
        int CurrentLevelIndex { get; }
        int DisplayLevelNumber { get; }
        int MaxLevel { get; }
        LevelDefinition GetLevelDefinition();
    }

    public interface ILevelProgressWriteModel
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
        public int MaxLevel => _levelDataService.LevelDefinitions?.Length ?? 0;

        public int CurrentLevelIndex => _levelProgressData.currentLevelIndex;
        public int DisplayLevelNumber
        {
            get
            {
                if (_levelDataService.UseInfiniteLevel) return _levelProgressData.displayLevelNumber;

                var defs = _levelDataService.LevelDefinitions;
                
                if (defs == null || defs.Length == 0) return 1;

                var idx = Mathf.Clamp(_levelProgressData.currentLevelIndex, 0, defs.Length - 1);
                
                return defs[idx].LevelNumber;
            }
        }

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
            
            EnsureInfiniteDisplayIsValid();
        }
        
        private void EnsureInfiniteDisplayIsValid()
        {
            if (!_levelDataService.UseInfiniteLevel) return;

            _levelProgressData.displayLevelNumber = Mathf.Max(_levelProgressData.displayLevelNumber, _levelProgressData.currentLevelIndex + 1);
        }

        private void Clamp()
        {
            _levelProgressData.currentLevelIndex = MaxLevel > 0 ? Mathf.Clamp(_levelProgressData.currentLevelIndex, 0, MaxLevel - 1) 
                                                           : Mathf.Max(0, _levelProgressData.currentLevelIndex);

            if (_levelDataService.UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = Mathf.Max(_levelProgressData.displayLevelNumber, 1);
        }

        public void SetLevel(int levelIndex)
        {
            if (MaxLevel <= 0) return;

            var clamped = Mathf.Clamp(levelIndex, 0, MaxLevel - 1);
            
            if (clamped == _levelProgressData.currentLevelIndex) return;

            _levelProgressData.currentLevelIndex = clamped;

            if (!_levelDataService.UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = _levelDataService.LevelDefinitions[clamped].LevelNumber;

            RaiseChanged();
        }

        public void AdvanceLevel()
        {
            if (MaxLevel <= 0) return;

            var next = _levelProgressData.currentLevelIndex + 1;
            
            next = next >= MaxLevel ? (_levelDataService.ResetLevelOnLimit ? 0 : MaxLevel - 1) : next;

            _levelProgressData.currentLevelIndex = next;

            if (_levelDataService.UseInfiniteLevel)
                _levelProgressData.displayLevelNumber++;

            RaiseChanged();
        }

        public void ResetProgress()
        {
            _levelProgressData.currentLevelIndex = 0;
            
            if (_levelDataService.UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = 1;

            RaiseChanged();
        }
        
        public LevelDefinition GetLevelDefinition() => _levelDataService.LevelDefinitions[CurrentLevelIndex];

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