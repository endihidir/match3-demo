using System;
using System.Collections.Generic;
using Core.Generated;
using Core.Item.Factories;
using Core.SceneService;
using VContainer.Unity;

namespace Core.Handlers
{
    public class GameplayFactoryCleaner : IInitializable, IDisposable
    {
        private readonly ISceneLoadState _sceneLoadState;
        private readonly IEnumerable<IFactoryCleaner> _factoryCleaners;
        
        public GameplayFactoryCleaner(ISceneLoadState sceneLoadState, IEnumerable<IFactoryCleaner> factoryCleaners)
        {
            _sceneLoadState = sceneLoadState;
            _factoryCleaners = factoryCleaners;
        }
        
        public void Initialize()
        {
            _sceneLoadState.OnLoadStart += OnSceneUnload;
        }
        
        private void OnSceneUnload()
        {
            if(_sceneLoadState.CurrentSceneGroupType != SceneGroupType.GameScene) return;
            
            foreach (var cleaner in _factoryCleaners)
            {
                cleaner.CleanupFactory();
            }
        }

        public void Dispose()
        {
            _sceneLoadState.OnLoadStart -= OnSceneUnload;
        }
    }
}