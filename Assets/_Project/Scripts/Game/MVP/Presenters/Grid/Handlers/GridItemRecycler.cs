using System;
using Core.Item.Factories;
using Core.SceneService;
using VContainer.Unity;

namespace Core.Handlers
{
    public class GridItemRecycler : IInitializable, IDisposable
    {
        private readonly IGridItemFactory _gridItemFactory;
        private readonly ISceneLoadState _sceneLoadState;
        public GridItemRecycler(IGridItemFactory gridItemFactory, ISceneLoadState sceneLoadState)
        {
            _gridItemFactory = gridItemFactory;
            _sceneLoadState = sceneLoadState;
        }

        public void Initialize() => _sceneLoadState.OnLoadStart += OnSceneUnload;
        private void OnSceneUnload() => _gridItemFactory.ReleaseAll();
        public void Dispose() => _sceneLoadState.OnLoadStart -= OnSceneUnload;
    }
}