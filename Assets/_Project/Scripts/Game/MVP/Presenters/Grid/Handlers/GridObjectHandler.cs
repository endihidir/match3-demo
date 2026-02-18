using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Utils;

namespace Core.Handlers
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
        
        public bool TryGetItem<T>(GridObjectType typeData, out T gridObject) where T : BaseGridObject
        {
            gridObject = null;
            
            var data = _gridConfigContainer.GetConfigData(typeData);

            if (!data) return false;

            gridObject = _gridObjectFactory.GetObject<T>();
            
            gridObject.Initialize(typeData)
                      .ApplyData(data);
            
            return true;
        }
        
        public void PopulateGridItems(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects)
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

        public BaseGridObject GetItem(GridObjectType typeData)
        {
            return typeData.ItemKind switch
            {
                GridItemKind.Regular => TryGetItem<ItemObject>(typeData, out var item) ? item : null,
                GridItemKind.Booster => TryGetItem<BoosterObject>(typeData, out var boosterObject) ? boosterObject : null,
                GridItemKind.Obstacle => TryGetItem<ObstacleObject>(typeData, out var obstacleObject) ? obstacleObject : null,
                _ => null
            };
        }

        public void ReleaseItem(BaseGridObject gridObject) => _gridObjectFactory.ReleaseObject(gridObject);
        public void ReleaseAllGridItems() => _gridObjectFactory.ReleaseObjectsByType<BaseGridObject>();

        // ---------- Typed creators ----------

        public ItemObject GetRegularItem(ItemType itemType)
        {
            var typeData = new GridObjectType(GridItemKind.Regular, (int)itemType);
            return TryGetItem<ItemObject>(typeData, out var item) ? item : null;
        }

        public ObstacleObject GetObstacleItem(ObstacleType obstacleType)
        {
            var typeData = new GridObjectType(GridItemKind.Obstacle, (int)obstacleType);
            return TryGetItem<ObstacleObject>(typeData, out var obstacleObject) ? obstacleObject : null;
        }
        
        public BoosterObject GetBoosterItem(BoosterType boosterType)
        {
            var typeData = new GridObjectType(GridItemKind.Booster, (int)boosterType);
            return TryGetItem<BoosterObject>(typeData, out var boosterObject) ? boosterObject : null;
        }
        
        // ---------- Random helpers ----------

        public ItemObject GetRandomItem()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            var typeData = new GridObjectType(GridItemKind.Regular, (int)randomType);
            return TryGetItem<ItemObject>(typeData, out var item) ? item : null;
        }

        public ObstacleObject GetRandomObstacle()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            var typeData = new GridObjectType(GridItemKind.Obstacle, (int)randomType);
            return TryGetItem<ObstacleObject>(typeData, out var obstacleObject) ? obstacleObject : null;
        }

        public BoosterObject GetRandomBooster()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            var typeData = new GridObjectType(GridItemKind.Booster, (int)randomType);
            return TryGetItem<BoosterObject>(typeData, out var boosterObject) ? boosterObject : null;;
        }
    }
}