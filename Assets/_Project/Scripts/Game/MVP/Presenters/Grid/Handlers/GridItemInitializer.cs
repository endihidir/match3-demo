using System;
using Core.Item;
using Core.Item.Factories;
using Core.SceneService;
using Core.Utils;
using VContainer.Unity;

namespace Core.Handlers
{
    public class GridItemInitializer : IInitializable, IGridItemCreator, IDisposable
    {
        private readonly IGridItemFactory _gridItemFactory;
        private readonly ISceneLoadState _sceneLoadState;
        public GridItemInitializer(IGridItemFactory gridItemFactory, ISceneLoadState sceneLoadState)
        {
            _gridItemFactory = gridItemFactory;
            _sceneLoadState = sceneLoadState;
        }

        public void CreateGridItems(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects)
        {
            itemObjects = new BaseGridObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coord = GridIndexUtil.ToCoord(i, width);
                var x = coord.x;
                var y = coord.y;
                var typeData = gridObjectTypes[x, y];
                
                if (typeData is { TypeId: -1 }) continue;
                itemObjects[x, y] = GetItem(typeData);
            }
        }

        private BaseGridObject GetItem(GridObjectType typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular => _gridItemFactory.GetItem<ItemObject>(typeData),
            GridItemKind.Booster => _gridItemFactory.GetItem<BoosterObject>(typeData),
            GridItemKind.Obstacle => _gridItemFactory.GetItem<ObstacleObject>(typeData),
            _ => null
        };

        public void Initialize() => _sceneLoadState.OnLoadStart += OnSceneUnload;
        private void OnSceneUnload() => _gridItemFactory.ReleaseAll();
        public void Dispose() => _sceneLoadState.OnLoadStart -= OnSceneUnload;
    }
}