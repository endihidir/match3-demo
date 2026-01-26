using System;
using System.Collections.Generic;
using Core.Generated;
using Core.Item.Factories;
using Core.SceneService;
using VContainer.Unity;

namespace Core.Handlers
{
    public class GameplayFactoryLifecycle : IInitializable, IDisposable
    {
        private readonly ISceneLoadState _sceneLoadState;
        private readonly IEnumerable<IFactoryResettable> _factoryResettables;
        
        public GameplayFactoryLifecycle(ISceneLoadState sceneLoadState, IEnumerable<IFactoryResettable> factoryResettables)
        {
            _sceneLoadState = sceneLoadState;
            _factoryResettables = factoryResettables;
        }
        
        public void Initialize()
        {
            _sceneLoadState.OnLoadStart += OnSceneUnload;
        }
        
        private void OnSceneUnload()
        {
            if(_sceneLoadState.CurrentSceneGroupType != SceneGroupType.GameScene) return;
            
            foreach (var factoryResettable in _factoryResettables)
            {
                factoryResettable.Reset();
            }
        }

        public void Dispose()
        {
            _sceneLoadState.OnLoadStart -= OnSceneUnload;
        }
    }
}