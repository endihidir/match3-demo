using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridObjectCreateHandler
    {
        void PopulateGrid(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
        bool TryCreateObject<T>(GridObjectType typeData, out T gridObject) where T : BaseGridObject;
        BaseGridObject CreateObject(GridObjectType typeData);
        ItemObject CreateItem(ItemType itemType);
        ObstacleObject CreateObstacle(ObstacleType obstacleType);
        BoosterObject CreateBooster(BoosterType boosterType);
        ItemObject CreateRandomItem();
        ObstacleObject CreateRandomObstacle();
        BoosterObject CreateRandomBooster();
    }
}