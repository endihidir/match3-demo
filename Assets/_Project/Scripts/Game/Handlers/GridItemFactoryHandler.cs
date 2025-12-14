using System;
using Core.Item;
using Core.Item.Factories;
using Core.Utils;

namespace Core.Handlers
{
    public interface IGridItemFactoryHandler
    {
        void PopulateGridWith(GridObjectTypeData[,] gridObjectTypes, out IGridItemObject[,] itemObjects);
    }

    public class GridItemFactoryHandler : IGridItemFactoryHandler, IDisposable
    {
        private readonly IGridItemFactory _gridItemFactory;
        
        public GridItemFactoryHandler(IGridItemFactory gridItemFactory)
        {
            _gridItemFactory = gridItemFactory;
        }
        
        public void PopulateGridWith(GridObjectTypeData[,] gridObjectTypes, out IGridItemObject[,] itemObjects)
        {
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            
            itemObjects = new IGridItemObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, width);
                var x = coordinate.x;
                var y = coordinate.y;
                var typeData = gridObjectTypes[x, y];
                
                if (typeData is { TypeId: -1 }) continue;

                var item = _gridItemFactory.GetItem(typeData, coordinate);
                
                itemObjects[x, y] = item;
            }
        }

        public void Dispose() => _gridItemFactory.ReleaseAllItems();
    }
}