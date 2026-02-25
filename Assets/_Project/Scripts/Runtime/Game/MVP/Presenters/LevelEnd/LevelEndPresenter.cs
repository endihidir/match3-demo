using System;
using Core.Scene.Services;
using Core.Generated;
using Game.Views;
using Game.Level.Models;
using Game.Services;
using Cysharp.Threading.Tasks;
using Game.Level.Handlers;
using VContainer.Unity;

namespace Game.Presenters
{
    public sealed class LevelEndPresenter : IInitializable, IDisposable
    {
        private readonly ILevelResultHandler _levelResultHandler;
        private readonly ILevelProgressionModel _progressionModel;
        private readonly ILevelEndView _levelEndView;
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IGameplaySetupService _gameplaySetupService;

        public LevelEndPresenter(ILevelProgressionModel progressionModel, ILevelEndView levelEndView, ILevelResultHandler levelResultHandler,
            ISceneLoadService sceneLoadService, IGameplaySetupService gameplaySetupService)
        {
            _levelResultHandler = levelResultHandler;
            _progressionModel = progressionModel;
            _levelEndView = levelEndView;
            _sceneLoadService = sceneLoadService;
            _gameplaySetupService = gameplaySetupService;
        }

        public void Initialize()
        {
            _levelResultHandler.OnLevelSuccess += OnLevelSuccess;
            _levelResultHandler.OnLevelFail += OnLevelFail;
            _levelEndView.OnClickNextButton.AddListener(OnClickNextButton);
            _levelEndView.OnClickTryAgainButton.AddListener(OnClickTryAgainButton);
        }

        private void OnClickNextButton() => _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene, true).Forget();
        private void OnClickTryAgainButton()
        {
            _levelEndView.CloseFailMenu();
            _gameplaySetupService.ResetGameplay();
        }

        private void OnLevelSuccess()
        {
            _levelEndView.OpenSuccessMenuViewAsync().Forget();
            _progressionModel.AdvanceLevel();
        }

        private void OnLevelFail() => _levelEndView.OpenFailMenuViewAsync().Forget();

        public void Dispose()
        {
            _levelEndView.OnClickTryAgainButton.RemoveListener(OnClickTryAgainButton);
            _levelEndView.OnClickNextButton.RemoveListener(OnClickNextButton);
            _levelResultHandler.OnLevelSuccess -= OnLevelSuccess;
            _levelResultHandler.OnLevelFail -= OnLevelFail;
        }
    }
}