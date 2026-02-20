using Game.Grid.Item;
using Game.Utils;

namespace Game.Grid.Handlers
{
    public sealed class GridObjectSpawnHandler : IGridObjectSpawnHandler
    {
        private readonly IGridObjectHandler _gridObjectHandler;

        public GridObjectSpawnHandler(IGridObjectHandler gridObjectHandler) => _gridObjectHandler = gridObjectHandler;

        public ItemObject GetRegularItem(ItemType itemType)
        {
            var typeData = new GridObjectType(GridItemKind.Regular, (int)itemType);
            return _gridObjectHandler.TryGetObject<ItemObject>(typeData, out var item) ? item : null;
        }

        public ObstacleObject GetObstacleItem(ObstacleType obstacleType)
        {
            var typeData = new GridObjectType(GridItemKind.Obstacle, (int)obstacleType);
            return _gridObjectHandler.TryGetObject<ObstacleObject>(typeData, out var obstacleObject) ? obstacleObject : null;
        }
        
        public BoosterObject GetBoosterItem(BoosterType boosterType)
        {
            var typeData = new GridObjectType(GridItemKind.Booster, (int)boosterType);
            return _gridObjectHandler.TryGetObject<BoosterObject>(typeData, out var boosterObject) ? boosterObject : null;
        }

        public ItemObject GetRandomItem()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            return GetRegularItem(randomType);
        }

        public ObstacleObject GetRandomObstacle()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            return GetObstacleItem(randomType);
        }

        public BoosterObject GetRandomBooster()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            return GetBoosterItem(randomType);
        }
    }
}