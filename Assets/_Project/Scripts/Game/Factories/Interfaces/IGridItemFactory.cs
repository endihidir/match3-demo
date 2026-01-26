using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectType typeData) where T : BaseGridObject;
        T GetItem<T>(GridItemKind itemKind, int typeId) where T : BaseGridObject => GetItem<T>(new GridObjectType(itemKind, typeId));
        public ItemObject GetRegularItem(ItemType itemType) => GetItem<ItemObject>(new GridObjectType(GridItemKind.Regular, (int)itemType));
        public BoosterObject GetBoosterItem(BoosterType boosterType) => GetItem<BoosterObject>(new GridObjectType(GridItemKind.Booster, (int)boosterType));
        public ObstacleObject GetObstacleItem(ObstacleType obstacleType) => GetItem<ObstacleObject>(new GridObjectType(GridItemKind.Obstacle, (int)obstacleType));
        ItemObject GetRandomItem();
        ObstacleObject GetRandomObstacle();
        BoosterObject GetRandomBooster();
        void ReleaseItem(BaseGridObject grid);
        void ReleaseItem(Transform item);
        void ReleaseItem(GameObject item) => ReleaseItem(item.transform);
        void ReleaseAllItemsOfType<T>() where T : BaseGridObject;
        void Remove();
    }
}