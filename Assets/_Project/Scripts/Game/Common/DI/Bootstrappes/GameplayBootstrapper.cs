using System;
using Core.SceneService;
using Core.Services;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameplayBootstrapper : IInitializable, IDisposable
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly IGameplaySetupService  _gameplaySetupService;
        
        public void Initialize() => _sceneLoadState.OnLoadComplete += OnSceneLoadComplete;

        private void OnSceneLoadComplete() => _gameplaySetupService.SetupGameplay();
        public void Dispose() => _sceneLoadState.OnLoadComplete -= OnSceneLoadComplete;
    }
}