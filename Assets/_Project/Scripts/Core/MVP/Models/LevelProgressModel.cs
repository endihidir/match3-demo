using System;
using Core.Level;
using Core.SaveSystem;
using UnityEngine;

namespace Core.Models
{
    public interface ILevelProgressReader
    {
        event Action OnProgressChanged;
        int CurrentLevelIndex { get; }
        int DisplayLevelNumber { get; }
    }

    public interface ILevelProgressWriter
    {
        void SetLevel(int levelIndex);
        void AdvanceLevel();
        void ResetProgress();
    }
    
    public sealed class LevelProgressModel : ILevelProgressReader, ILevelProgressWriter
    {
        private const string SaveKey = "level_progress";

        private readonly IJsonSaveService _saveService;
        private readonly ILevelDataReader _levelDataReader;

        private LevelProgressData _levelProgressData;
        public int MaxLevel => _levelDataReader.LevelSize;
        public int CurrentLevelIndex => _levelProgressData.currentLevelIndex;
        public int DisplayLevelNumber => _levelProgressData.displayLevelNumber;
        private bool ResetIndexOnLimit => true; //TODO: Get this form config
        public event Action OnProgressChanged;
        public LevelProgressModel(IJsonSaveService saveService, ILevelDataReader levelDataReader)
        {
            _saveService = saveService;
            _levelDataReader = levelDataReader;

            var defaultState = new LevelProgressData
            {
                currentLevelIndex = 0,
                displayLevelNumber = 1
            };
            
            _levelProgressData = _saveService.LoadFromTextFile(SaveKey, defaultState);
            
            _levelProgressData.currentLevelIndex = Mathf.Clamp(_levelProgressData.currentLevelIndex, 0, MaxLevel - 1);
        }

        public void SetLevel(int levelIndex)
        {
            if (MaxLevel <= 0) return;

            var index = Mathf.Clamp(levelIndex, 0, MaxLevel - 1);
            
            if (index == _levelProgressData.currentLevelIndex) return;

            _levelProgressData.currentLevelIndex = index;
            
            RaiseChanged();
        }

        public void AdvanceLevel()
        {
            if (MaxLevel <= 0) return;

            var next = _levelProgressData.currentLevelIndex + 1;
            
            next = next >= MaxLevel ? (ResetIndexOnLimit ? 0 : MaxLevel - 1) : next;
            
            _levelProgressData.currentLevelIndex = next;
            
            _levelProgressData.displayLevelNumber++;
            
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
            _saveService.SaveToTextFile(SaveKey, _levelProgressData);
            OnProgressChanged?.Invoke();
        }

        [Serializable]
        private struct LevelProgressData
        {
            public int currentLevelIndex;
            public int displayLevelNumber;
        }
    }
}