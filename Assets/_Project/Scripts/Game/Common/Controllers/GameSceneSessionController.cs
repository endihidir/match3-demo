using System;
using Core.SceneService;
using Core.Services;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameSceneSessionController : IInitializable, IDisposable
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly IGameplaySetupService  _gameplaySetupService;
        
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