using Game.Configs;
using Game.Grid.Item;
using Game.Grid.Item.Factories;
using Game.Utils;

namespace Game.Grid.Handlers
{
    public sealed class GridObjectHandler : IGridObjectHandler
    {
        private readonly IGridObjectFactory _gridObjectFactory;
        private readonly GridConfigContainerSO _gridConfigContainer;
        
        public GridObjectHandler(IGridObjectFactory gridObjectFactory, GridConfigContainerSO gridConfigContainer)
        {
            _gridObjectFactory = gridObjectFactory;
            _gridConfigContainer = gridConfigContainer;
        }
        
        public bool TryGetObject<T>(GridObjectType typeData, out T gridObject) where T : BaseGridObject
        {
            gridObject = null;
            
            var data = _gridConfigContainer.GetConfigData(typeData);

            if (!data) return false;

            gridObject = _gridObjectFactory.GetObject<T>();

            if (!gridObject) return false;
            
            gridObject.Initialize(typeData)
                      .ApplyData(data);
            
            return true;
        }
        
        public void PopulateGridObjects(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects)
        {
            itemObjects = new BaseGridObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coord = GridIndexUtil.ToCoord(i, width);
                var x = coord.x;
                var y = coord.y;

                var typeData = gridObjectTypes[x, y];
                if (typeData is { TypeId: -1 }) continue;
                
                itemObjects[x, y] = GetObject(typeData);
            }
        }

        public BaseGridObject GetObject(GridObjectType typeData)
        {
            return typeData.ItemKind switch
            {
                GridItemKind.Regular => TryGetObject<ItemObject>(typeData, out var item) ? item : null,
                GridItemKind.Booster => TryGetObject<BoosterObject>(typeData, out var boosterObject) ? boosterObject : null,
                GridItemKind.Obstacle => TryGetObject<ObstacleObject>(typeData, out var obstacleObject) ? obstacleObject : null,
                _ => null
            };
        }
    }
}