using Game.Configs;
using Game.Grid.Item;
using Game.Grid.Item.Factories;
using Game.Utils;

namespace Game.Grid.Handlers
{
    public sealed class GridObjectCreateHandler : IGridObjectCreateHandler
    {
        private readonly IGridObjectFactory _gridObjectFactory;
        private readonly GridConfigContainerSO _gridConfigContainer;
        
        public GridObjectCreateHandler(IGridObjectFactory gridObjectFactory, GridConfigContainerSO gridConfigContainer)
        {
            _gridObjectFactory = gridObjectFactory;
            _gridConfigContainer = gridConfigContainer;
        }
        
        public void PopulateGrid(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects)
        {
            itemObjects = new BaseGridObject[width, height];

            for (int i = 0; i < width * height; i++)
            {
                var coord = GridIndexUtil.ToCoord(i, width);
                var x = coord.x;
                var y = coord.y;

                var typeData = gridObjectTypes[x, y];
                if (typeData is { TypeId: -1 }) continue;
                
                itemObjects[x, y] = CreateObject(typeData);
            }
        }

        public BaseGridObject CreateObject(GridObjectType typeData)
        {
            return typeData.ItemKind switch
            {
                GridItemKind.Regular => TryCreateObject<ItemObject>(typeData, out var item) ? item : null,
                GridItemKind.Booster => TryCreateObject<BoosterObject>(typeData, out var boosterObject) ? boosterObject : null,
                GridItemKind.Obstacle => TryCreateObject<ObstacleObject>(typeData, out var obstacleObject) ? obstacleObject : null,
                _ => null
            };
        }
        
        public bool TryCreateObject<T>(GridObjectType typeData, out T gridObject, bool activate = true) where T : BaseGridObject
        {
            gridObject = null;
            
            var data = _gridConfigContainer.GetConfigData(typeData);

            if (!data) return false;

            gridObject = _gridObjectFactory.GetObject<T>(activate);

            if (!gridObject) return false;
            
            gridObject.Initialize(typeData)
                      .ApplyData(data);
            
            return true;
        }
        
        public ItemObject CreateItem(ItemType itemType, bool activate = true)
        {
            var typeData = new GridObjectType(GridItemKind.Regular, (int)itemType);
            return TryCreateObject<ItemObject>(typeData, out var item, activate) ? item : null;
        }

        public ObstacleObject CreateObstacle(ObstacleType obstacleType, bool activate = true)
        {
            var typeData = new GridObjectType(GridItemKind.Obstacle, (int)obstacleType);
            return TryCreateObject<ObstacleObject>(typeData, out var obstacleObject, activate) ? obstacleObject : null;
        }
        
        public BoosterObject CreateBooster(BoosterType boosterType, bool activate = true)
        {
            var typeData = new GridObjectType(GridItemKind.Booster, (int)boosterType);
            return TryCreateObject<BoosterObject>(typeData, out var boosterObject, activate) ? boosterObject : null;
        }

        public ItemObject CreateRandomItem(bool activate = true)
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            return CreateItem(randomType, activate);
        }

        public ObstacleObject CreateRandomObstacle(bool activate = true)
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            return CreateObstacle(randomType, activate);
        }

        public BoosterObject CreateRandomBooster(bool activate = true)
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            return CreateBooster(randomType, activate);
        }
    }
}