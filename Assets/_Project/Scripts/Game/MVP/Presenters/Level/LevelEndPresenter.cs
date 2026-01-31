using System;
using System.Linq;
using Core.Handlers;
using Core.Item;
using Core.UI;
using VContainer.Unity;

namespace Core.Models
{
    public class LevelEndPresenter : IInitializable, IDisposable
    {
        private readonly IGridModel _gridModel;
        private readonly ILevelObjectiveModel _objectiveModel;
        private readonly ILevelProgressionModel _progressionModel;
        private readonly ILevelEndView _levelEndView;
        private readonly IGridStateHandler _stateHandler;

        public LevelEndPresenter(IGridModel model, ILevelObjectiveModel objectiveModel, ILevelProgressionModel progressionModel, ILevelEndView levelEndView, 
            IGridStateHandler stateHandler)
        {
            _gridModel = model;
            _objectiveModel = objectiveModel;
            _progressionModel = progressionModel;
            _levelEndView = levelEndView;
            _stateHandler = stateHandler;
        }

        public void Initialize()
        {
            _objectiveModel.OnGoalsComplete += OnLevelCompleted;
            _stateHandler.Context.OnGridDestructionComplete += OnGridDestructionComplete;
        }

        private void OnLevelCompleted()
        {
            _levelEndView.OpenSuccessMenuView();
            _progressionModel.AdvanceLevel();
        }

        private void OnGridDestructionComplete()
        {
            if(!_objectiveModel.IsAllMovesFinished) return;
            
            var itemTypes = _gridModel.BuildGridTypeDataArray();

            if (itemTypes.Count(x => x.ItemKind == GridItemKind.Obstacle) > 0)
            {
                _levelEndView.OpenFailMenuView();
            }
        }
        
        public void Dispose()
        {
            _objectiveModel.OnGoalsComplete -= OnLevelCompleted;
            _stateHandler.Context.OnGridDestructionComplete -= OnGridDestructionComplete;
        }
    }
}