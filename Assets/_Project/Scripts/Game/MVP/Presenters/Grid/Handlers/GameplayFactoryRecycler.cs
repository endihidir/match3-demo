using System;
using Core.SceneService;
using Core.Services;
using VContainer;
using VContainer.Unity;

namespace Core.Handlers
{
    public class GameplayFactoryRecycler : IInitializable, IDisposable
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly IGameplaySetupService _gameplaySetupService;

        public void Initialize() => _sceneLoadState.OnLoadStart += OnSceneUnload;
        private void OnSceneUnload() => _gameplaySetupService.ReleaseFactories();
        public void Dispose() => _sceneLoadState.OnLoadStart -= OnSceneUnload;
    }
}