using Core.Utils;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectType typeData) where T : BaseGridObject;
        void ReleaseItem(BaseGridObject grid);
        void ReleaseItem(Transform item);
        void ReleaseItem(GameObject item) => ReleaseItem(item.transform);
        void ReleaseAllItemsOfType<T>() where T : BaseGridObject;
        void ReleaseAll();
        void Remove();
        
        T GetItem<T>(GridItemKind itemKind, int typeId) where T : BaseGridObject
        {
            var type = new GridObjectType(itemKind, typeId);
            return GetItem<T>(new GridObjectType(itemKind, typeId));
        }

        public ItemObject GetRegularItem(ItemType itemType)
        {
            var type = new GridObjectType(GridItemKind.Regular, (int)itemType);
            return GetItem<ItemObject>(type);
        }

        public BoosterObject GetBoosterItem(BoosterType boosterType)
        {
            var type = new GridObjectType(GridItemKind.Booster, (int)boosterType);
            return GetItem<BoosterObject>(new GridObjectType(GridItemKind.Booster, (int)boosterType));
        }

        public ObstacleObject GetObstacleItem(ObstacleType obstacleType)
        {
            var type = new GridObjectType(GridItemKind.Obstacle, (int)obstacleType);
            return GetItem<ObstacleObject>(type);
        }

        ItemObject GetRandomItem()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Regular, (int)randomType);
            return GetItem<ItemObject>(objTypeData);
        }

        ObstacleObject GetRandomObstacle()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Obstacle, (int)randomType);
            return GetItem<ObstacleObject>(objTypeData);
        }

        BoosterObject GetRandomBooster()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Booster, (int)randomType);
            return GetItem<BoosterObject>(objTypeData);
        }
    }
}