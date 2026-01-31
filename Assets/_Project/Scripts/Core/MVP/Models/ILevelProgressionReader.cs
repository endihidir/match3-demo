using System;

namespace Core.Models
{
    public interface ILevelProgressionReader
    {
        event Action OnProgressChanged;
        int CurrentLevelIndex { get; }
        int DisplayLevelNumber { get; }
    }
}