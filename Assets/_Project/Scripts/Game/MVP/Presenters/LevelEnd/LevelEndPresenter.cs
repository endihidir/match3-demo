using System;
using System.Linq;
using Core.Scene.Services;
using Core.Generated;
using Game.Views;
using Game.Grid.Item;
using Game.Grid.Handlers;
using Game.Level.Models;
using Game.Models;
using Game.Services;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Game.Presenters
{
    public sealed class LevelEndPresenter : IInitializable, IDisposable
    {
        private readonly IGridModel _gridModel;
        private readonly ILevelObjectiveModel _objectiveModel;
        private readonly ILevelProgressionModel _progressionModel;
        private readonly ILevelEndView _levelEndView;
        private readonly IGridStateHandler _stateHandler;
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IGameplaySetupService _gameplaySetupService;
        private bool _isLevelFailed = false;

        public LevelEndPresenter(IGridModel model, ILevelObjectiveModel objectiveModel, ILevelProgressionModel progressionModel, ILevelEndView levelEndView, 
            IGridStateHandler stateHandler, ISceneLoadService sceneLoadService, IGameplaySetupService gameplaySetupService)
        {
            _gridModel = model;
            _objectiveModel = objectiveModel;
            _progressionModel = progressionModel;
            _levelEndView = levelEndView;
            _stateHandler = stateHandler;
            _sceneLoadService = sceneLoadService;
            _gameplaySetupService = gameplaySetupService;
        }

        public void Initialize()
        {
            _objectiveModel.OnGoalsComplete += OnLevelCompleted;
            _stateHandler.Context.OnDestructionStateComplete += OnDestructionStateComplete;
            _levelEndView.OnClickNextButton.AddListener(OnClickNextButton);
            _levelEndView.OnClickTryAgainButton.AddListener(OnClickTryAgainButton);
        }

        private void OnClickNextButton() => _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene, true).Forget();
        private void OnClickTryAgainButton()
        {
            _isLevelFailed = false;
            _levelEndView.CloseFailMenu();
            _gameplaySetupService.ResetGameplay();
        }

        private void OnLevelCompleted()
        {
            _levelEndView.OpenSuccessMenuViewAsync().Forget();
            _progressionModel.AdvanceLevel();
        }

        private void OnDestructionStateComplete()
        {
            if(!_objectiveModel.IsAllMovesFinished) return;
            
            if(_isLevelFailed) return;
            
            var itemTypes = _gridModel.BuildGridTypeDataArray();

            if (itemTypes.Count(x => x.ItemKind == GridItemKind.Obstacle) <= 0) return;
            
            _isLevelFailed = true;
                
            _levelEndView.OpenFailMenuViewAsync().Forget();
        }
        
        public void Dispose()
        {
            _levelEndView.OnClickTryAgainButton.RemoveListener(OnClickTryAgainButton);
            _levelEndView.OnClickNextButton.RemoveListener(OnClickNextButton);
            _objectiveModel.OnGoalsComplete -= OnLevelCompleted;
            _stateHandler.Context.OnDestructionStateComplete -= OnDestructionStateComplete;
        }
    }
}