using System;
using System.Linq;
using Game.Grid.Handlers;
using Game.Grid.Item;
using Game.Level.Models;
using Game.Models;

namespace Game.Level.Handlers
{
    public sealed class LevelResultHandler : ILevelResultHandler, IDisposable
    {
        private readonly ILevelGoalModel _goalModel;
        private readonly IGridModel _gridModel;
        private readonly IGridStateHandler _gridStateHandler;

        public event Action OnLevelSuccess;
        public event Action OnLevelFail;

        public LevelResultHandler(ILevelGoalModel goalModel, IGridModel gridModel, IGridStateHandler gridStateHandler)
        {
            _goalModel = goalModel;
            _gridModel = gridModel;
            _gridStateHandler = gridStateHandler;
            
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

            var hasObstacles = _gridModel.BuildGridTypeDataArray().Any(x => x.ObjectKind == GridObjectKind.Obstacle);
            
            if (!hasObstacles) return;
            
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