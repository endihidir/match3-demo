using System;
using System.Collections.Generic;
using Core.Item.Factories;
using Core.SceneService;
using VContainer.Unity;

namespace Core.Handlers
{
    public class GameplayFactoryCleaner : IInitializable, IDisposable
    {
        private readonly ISceneLoadContext _sceneLoadContext;
        private readonly IEnumerable<IFactoryCleaner> _factoryCleaners;
        
        public GameplayFactoryCleaner(ISceneLoadContext sceneLoadContext, IEnumerable<IFactoryCleaner> factoryCleaners)
        {
            _sceneLoadContext = sceneLoadContext;
            _factoryCleaners = factoryCleaners;
        }
        
        public void Initialize()
        {
            _sceneLoadContext.OnLoadStart += OnSceneUnload;
        }
        
        private void OnSceneUnload()
        {
            foreach (var cleaner in _factoryCleaners)
            {
                cleaner.CleanupFactory();
            }
        }

        public void Dispose()
        {
            _sceneLoadContext.OnLoadStart -= OnSceneUnload;
        }
    }
}