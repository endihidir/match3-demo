using System;
using System.Linq;
using Core.Generated;
using Core.Handlers;
using Core.Item;
using Core.SceneService;
using Core.UI;
using Cysharp.Threading.Tasks;
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
        private readonly ISceneLoadService _sceneLoadService;

        public LevelEndPresenter(IGridModel model, ILevelObjectiveModel objectiveModel, ILevelProgressionModel progressionModel, ILevelEndView levelEndView, 
            IGridStateHandler stateHandler, ISceneLoadService sceneLoadService)
        {
            _gridModel = model;
            _objectiveModel = objectiveModel;
            _progressionModel = progressionModel;
            _levelEndView = levelEndView;
            _stateHandler = stateHandler;
            _sceneLoadService = sceneLoadService;
        }

        public void Initialize()
        {
            _objectiveModel.OnGoalsComplete += OnLevelCompleted;
            _stateHandler.Context.OnGridDestructionComplete += OnGridDestructionComplete;
            _levelEndView.OnClickNextButton.AddListener(OnClickNextButton);
            _levelEndView.OnClickTryAgainButton.AddListener(OnClickTryAgainButton);
        }

        private void OnClickNextButton() => _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene, true).Forget();
        private void OnClickTryAgainButton() => _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene, true).Forget();

        private void OnLevelCompleted()
        {
            _levelEndView.OpenSuccessMenuViewAsync().Forget();
            _progressionModel.AdvanceLevel();
        }

        private void OnGridDestructionComplete()
        {
            if(!_objectiveModel.IsAllMovesFinished) return;
            
            var itemTypes = _gridModel.BuildGridTypeDataArray();

            if (itemTypes.Count(x => x.ItemKind == GridItemKind.Obstacle) > 0)
            {
                _levelEndView.OpenFailMenuViewAsync().Forget();
            }
        }
        
        public void Dispose()
        {
            _levelEndView.OnClickTryAgainButton.RemoveListener(OnClickTryAgainButton);
            _levelEndView.OnClickNextButton.RemoveListener(OnClickNextButton);
            _objectiveModel.OnGoalsComplete -= OnLevelCompleted;
            _stateHandler.Context.OnGridDestructionComplete -= OnGridDestructionComplete;
        }
    }
}