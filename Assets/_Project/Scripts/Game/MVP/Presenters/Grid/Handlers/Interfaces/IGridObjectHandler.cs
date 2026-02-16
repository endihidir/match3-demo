using Core.Item;

namespace Core.Handlers
{
    public interface IGridObjectHandler
    {
        bool TryGetItem<T>(GridObjectType typeData, out T gridObject) where T : BaseGridObject;
        void PopulateGridItems(GridObjectType[,] gridObjectTypes, int width, int height, out BaseGridObject[,] itemObjects);
        BaseGridObject GetItem(GridObjectType typeData);
        void ReleaseItem(BaseGridObject gridObject);
        void ReleaseAllGridItems();
        ItemObject GetRegularItem(ItemType itemType);
        ObstacleObject GetObstacleItem(ObstacleType obstacleType);
        BoosterObject GetBoosterItem(BoosterType boosterType);
        ItemObject GetRandomItem();
        ObstacleObject GetRandomObstacle();
        BoosterObject GetRandomBooster();
    }
}