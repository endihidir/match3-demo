using System;
using Core.Item;
using Core.Item.Factories;
using Core.SceneService;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public interface IGridItemFactoryHandler
    {
        void PopulateGridWith(GridObjectTypeData[,] gridObjectTypes, out BaseItemObject[,] itemObjects);
    }

    public class GridItemFactoryHandler : IGridItemFactoryHandler, IDisposable
    {
        private readonly IGridItemFactory _gridItemFactory;
        private readonly ISceneLoadContext _sceneLoadContext;
        
        public GridItemFactoryHandler(IGridItemFactory gridItemFactory, ISceneLoadContext sceneLoadContext)
        {
            _gridItemFactory = gridItemFactory;
            _sceneLoadContext = sceneLoadContext;
            _sceneLoadContext.OnLoadStart += OnSceneLoadStart;
        }

        public void PopulateGridWith(GridObjectTypeData[,] gridObjectTypes, out BaseItemObject[,] itemObjects)
        {
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            
            itemObjects = new BaseItemObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, width);
                var x = coordinate.x;
                var y = coordinate.y;
                var typeData = gridObjectTypes[x, y];
                
                if (typeData is { TypeId: -1 }) continue;

                var item = GetItem(typeData, coordinate);
                
                itemObjects[x, y] = item;
            }
        }

        private BaseItemObject GetItem(GridObjectTypeData gridObjectTypeData, Vector2Int coordinate) => gridObjectTypeData.ItemKind switch
        {
            GridItemKind.Regular => _gridItemFactory.GetItem<ItemObject>(gridObjectTypeData, coordinate),
            GridItemKind.Booster => _gridItemFactory.GetItem<BoosterObject>(gridObjectTypeData, coordinate),
            GridItemKind.Obstacle => _gridItemFactory.GetItem<ObstacleObject>(gridObjectTypeData, coordinate),
            _ => null
        };
        
        private void OnSceneLoadStart()
        {
            _gridItemFactory.ReleaseAllItems();
        }

        public void Dispose()
        {
            _sceneLoadContext.OnLoadStart -= OnSceneLoadStart;
        }
    }
}