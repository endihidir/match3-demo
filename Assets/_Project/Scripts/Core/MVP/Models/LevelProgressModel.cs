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
        int MaxLevel { get; }
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

        private readonly IDataPersistenceService _persistence;
        private readonly ILevelDataReader _levelDataReader;

        private LevelProgressData _levelProgressData;
        public int MaxLevel => _levelDataReader.MaxSize;

        public int CurrentLevelIndex => _levelProgressData.currentLevelIndex;
        private bool ResetLevelOnLimit => true; //TODO: Get this form config
        private bool UseInfiniteLevel => true; //TODO: Get this form config
        
        public int DisplayLevelNumber
        {
            get
            {
                if (UseInfiniteLevel) return _levelProgressData.displayLevelNumber;

                var index = Mathf.Clamp(_levelProgressData.currentLevelIndex, 0, MaxLevel - 1);
                
                return _levelDataReader.GetLevelNumber(index);
            }
        }

        public event Action OnProgressChanged;

        public LevelProgressModel(IDataPersistenceService persistence, ILevelDataReader levelDataReader)
        {
            _persistence = persistence;
            _levelDataReader = levelDataReader;

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
            if (!UseInfiniteLevel) return;

            _levelProgressData.displayLevelNumber = Mathf.Max(_levelProgressData.displayLevelNumber, _levelProgressData.currentLevelIndex + 1);
        }

        private void Clamp()
        {
            _levelProgressData.currentLevelIndex = MaxLevel > 0 ? Mathf.Clamp(_levelProgressData.currentLevelIndex, 0, MaxLevel - 1) 
                                                           : Mathf.Max(0, _levelProgressData.currentLevelIndex);

            if (UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = Mathf.Max(_levelProgressData.displayLevelNumber, 1);
        }

        public void SetLevel(int levelIndex)
        {
            if (MaxLevel <= 0) return;

            var index = Mathf.Clamp(levelIndex, 0, MaxLevel - 1);
            
            if (index == _levelProgressData.currentLevelIndex) return;

            _levelProgressData.currentLevelIndex = index;

            if (!UseInfiniteLevel)
                _levelProgressData.displayLevelNumber = _levelDataReader.GetLevelNumber(index);

            RaiseChanged();
        }

        public void AdvanceLevel()
        {
            if (MaxLevel <= 0) return;

            var next = _levelProgressData.currentLevelIndex + 1;
            
            next = next >= MaxLevel ? (ResetLevelOnLimit ? 0 : MaxLevel - 1) : next;

            _levelProgressData.currentLevelIndex = next;

            if (UseInfiniteLevel)
                _levelProgressData.displayLevelNumber++;

            RaiseChanged();
        }

        public void ResetProgress()
        {
            _levelProgressData.currentLevelIndex = 0;
            
            if (UseInfiniteLevel)
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