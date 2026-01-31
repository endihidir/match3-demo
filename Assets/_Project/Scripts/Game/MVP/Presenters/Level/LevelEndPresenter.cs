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
        private readonly ILevelEndView _levelEndView;
        private readonly IGridStateHandler _gridStateHandler;

        public LevelEndPresenter(IGridModel model, ILevelObjectiveModel objectiveModel, ILevelEndView levelEndView, IGridStateHandler gridStateHandler)
        {
            _gridModel = model;
            _objectiveModel = objectiveModel;
            _levelEndView = levelEndView;
            _gridStateHandler = gridStateHandler;
        }

        public void Initialize()
        {
            _objectiveModel.OnAllGoalsComplete += OnAllGoalsComplete;
            _gridStateHandler.Context.OnGridDestructionComplete += OnGridDestructionComplete;
        }

        private void OnAllGoalsComplete() => _levelEndView.OpenLevelSuccessPanel();
        
        private void OnGridDestructionComplete()
        {
            if(!_objectiveModel.IsAllMovesFinished) return;
            
            var itemTypes = _gridModel.BuildGridTypeDataArray();

            if (itemTypes.Count(x => x.ItemKind == GridItemKind.Obstacle) > 0)
            {
                _levelEndView.OpenLevelFailPanel();
            }
        }
        
        public void Dispose()
        {
            _objectiveModel.OnAllGoalsComplete -= OnAllGoalsComplete;
            _gridStateHandler.Context.OnGridDestructionComplete -= OnGridDestructionComplete;
        }
    }
}