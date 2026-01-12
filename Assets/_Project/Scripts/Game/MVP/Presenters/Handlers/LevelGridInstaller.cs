using Core.Item;
using Core.Item.Factories;
using Core.Utils;

namespace Core.Handlers
{
    public class LevelGridInstaller : ILevelGridInstaller
    {
        private readonly IGridItemFactory _gridItemFactory;
        
        public LevelGridInstaller(IGridItemFactory gridItemFactory) => _gridItemFactory = gridItemFactory;

        public void PopulateGrid(GridObjectType[,] gridObjectTypes, out BaseGridObject[,] itemObjects)
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

        private BaseGridObject GetItem(GridObjectType typeData) => typeData.ItemKind switch
        {
            GridItemKind.Regular => _gridItemFactory.GetItem<ItemObject>(typeData),
            GridItemKind.Booster => _gridItemFactory.GetItem<BoosterObject>(typeData),
            GridItemKind.Obstacle => _gridItemFactory.GetItem<ObstacleObject>(typeData),
            _ => null
        };
    }
}