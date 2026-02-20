using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridObjectSpawnHandler
    {
        ItemObject GetRegularItem(ItemType itemType);
        ObstacleObject GetObstacleItem(ObstacleType obstacleType);
        BoosterObject GetBoosterItem(BoosterType boosterType);
        ItemObject GetRandomItem();
        ObstacleObject GetRandomObstacle();
        BoosterObject GetRandomBooster();
    }
}