using System;
using Core.SceneService;
using Core.Services;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameSceneSessionController : IInitializable, IDisposable
    {
        private readonly ISceneLoadState _sceneLoadState;
        private readonly IGameplaySetupService  _gameplaySetupService;
        public GameSceneSessionController(ISceneLoadState sceneLoadState, IGameplaySetupService gameplaySetupService)
        {
            _sceneLoadState = sceneLoadState;
            _gameplaySetupService = gameplaySetupService;
        }
        
        public void Initialize()
        {
            _sceneLoadState.OnLoadComplete += OnGameSceneLoadComplete;
            _sceneLoadState.OnLoadStart += OnUnloadGameScene;
        }

        private void OnGameSceneLoadComplete() => _gameplaySetupService.SetupGameplay();
        private void OnUnloadGameScene() => _gameplaySetupService.ReleaseFactories();
        
        public void Dispose()
        {
            _sceneLoadState.OnLoadComplete -= OnGameSceneLoadComplete;
            _sceneLoadState.OnLoadStart -= OnUnloadGameScene;
        }
    }
}