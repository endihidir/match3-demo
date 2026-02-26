using System;

namespace Game.Level.Handlers
{
    public interface ILevelResultHandler
    {
        event Action OnLevelSuccess;
        event Action OnLevelFail;
    }
}