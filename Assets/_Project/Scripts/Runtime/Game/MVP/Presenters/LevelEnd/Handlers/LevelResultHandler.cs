using System;
using Game.Grid.Handlers;
using Game.Level.Models;

namespace Game.Level.Handlers
{
    public sealed class LevelResultHandler : ILevelResultHandler, IDisposable
    {
        private readonly ILevelGoalModel _goalModel;
        private readonly IGridStateHandler _gridStateHandler;

        public event Action OnLevelSuccess;
        public event Action OnLevelFail;
        
        public LevelResultHandler(ILevelGoalModel goalModel, IGridStateHandler gridStateHandler)
        {
            _goalModel = goalModel;
            _gridStateHandler = gridStateHandler;
        }
        
        public void Initialize()
        {
            _goalModel.OnGoalCountUpdate += HandleSuccess;
            _gridStateHandler.OnDestructionStateComplete += HandleFail;
        }
        
        private void HandleSuccess()
        {
            if(!_goalModel.IsAllGoalsComplete) return;
            
            Unsubscribe();
            
            OnLevelSuccess?.Invoke();
        }

        private void HandleFail()
        {
            if (!_goalModel.IsAllMovesFinished) return;
            
            if (_goalModel.IsAllGoalsComplete) return;
            
            Unsubscribe();
            
            OnLevelFail?.Invoke();
        }

        private void Unsubscribe()
        {
            _goalModel.OnGoalCountUpdate -= HandleSuccess;
            _gridStateHandler.OnDestructionStateComplete -= HandleFail;
        }

        public void Dispose() => Unsubscribe();
    }
}