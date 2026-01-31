using System;

namespace Core.Models
{
    public interface ILevelProgressionReader
    {
        event Action OnLevelChanged;
        int CurrentLevelIndex { get; }
        int DisplayLevelNumber { get; }
    }
}