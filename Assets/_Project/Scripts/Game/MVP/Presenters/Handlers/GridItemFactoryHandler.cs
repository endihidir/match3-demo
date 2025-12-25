using System;
using Core.Item;
using Core.Item.Factories;
using Core.SceneService;
using Core.Utils;

namespace Core.Handlers
{
    public class GridItemFactoryHandler : IGridItemFactoryHandler, IDisposable
    {
        private readonly IGridItemFactory _gridItemFactory;
        private readonly ISceneLoadContext _sceneLoadContext;
        
        public GridItemFactoryHandler(IGridItemFactory gridItemFactory, ISceneLoadContext sceneLoadContext)
        {
            _gridItemFactory = gridItemFactory;
            _sceneLoadContext = sceneLoadContext;
            _sceneLoadContext.OnLoadStart += OnSceneUnload;
        }

        public void PopulateGridWith(GridObjectType[,] gridObjectTypes, out BaseGridObject[,] itemObjects)
        {
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            
            itemObjects = new BaseGridObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coordinate = GridIndexUtil.ToCoord(i, width);
                var x = coordinate.x;
                var y = coordinate.y;
                var typeData = gridObjectTypes[x, y];
                
                if (typeData is { TypeId: -1 }) continue;
                
                itemObjects[x, y] = GetItem(typeData);
            }
        }

        private BaseGridObject GetItem(GridObjectType ıd) => ıd.ItemKind switch
        {
            GridItemKind.Regular => _gridItemFactory.GetItem<GridObject>(ıd),
            GridItemKind.Booster => _gridItemFactory.GetItem<BoosterObject>(ıd),
            GridItemKind.Obstacle => _gridItemFactory.GetItem<ObstacleObject>(ıd),
            _ => null
        };
        
        private void OnSceneUnload()
        {
            _gridItemFactory.ReleaseAllItems();
        }

        public void Dispose()
        {
            _sceneLoadContext.OnLoadStart -= OnSceneUnload;
        }
    }
}