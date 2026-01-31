using System;

namespace Core.Models
{
    public interface ILevelProgressionModel
    {
        event Action OnLevelChanged;
        int CurrentLevelIndex { get; }
        int DisplayLevelNumber { get; }
        void SetLevel(int levelIndex);
        void AdvanceLevel();
        void ResetProgress();
    }
}