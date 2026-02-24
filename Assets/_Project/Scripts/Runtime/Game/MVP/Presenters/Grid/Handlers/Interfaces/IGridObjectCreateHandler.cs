using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridObjectCreateHandler
    {
        void PopulateGrid(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
        bool TryCreateObject<T>(GridObjectType typeData, out T gridObject, bool activate = true) where T : BaseGridObject;
        BaseGridObject CreateObject(GridObjectType typeData);
        ItemObject CreateItem(ItemType itemType, bool activate = true);
        ObstacleObject CreateObstacle(ObstacleType obstacleType, bool activate = true);
        BoosterObject CreateBooster(BoosterType boosterType, bool activate = true);
        ItemObject CreateRandomItem(bool activate = true);
        ObstacleObject CreateRandomObstacle(bool activate = true);
        BoosterObject CreateRandomBooster(bool activate = true);
    }
}