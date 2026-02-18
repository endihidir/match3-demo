using System;
using Core.Scene.Services;
using Game.Services;
using VContainer.Unity;

namespace Core.Bootstrappers
{
    public class GameplayBootstrapper : IInitializable, IDisposable
    {
        private readonly ISceneLoadState _sceneLoadState;
        private readonly IGameplaySetupService  _gameplaySetupService;
        public GameplayBootstrapper(ISceneLoadState sceneLoadState, IGameplaySetupService gameplaySetupService)
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