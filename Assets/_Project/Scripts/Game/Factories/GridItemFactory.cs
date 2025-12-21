using Core.Config;
using Core.Configs;
using Core.Pool;
using Core.Utils;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectTypeData typeData) where T : BaseItemObject;
        T GetItem<T>(GridItemKind itemKind, int typeId) where T : BaseItemObject => GetItem<T>(new GridObjectTypeData(itemKind, typeId));
        ItemObject GetRandomItem();
        ObstacleObject GetRandomObstacle();
        BoosterObject GetRandomBooster();
        void ReleaseItem(BaseItemObject item);
        void ReleaseItem(Transform item);
        void ReleaseItem(GameObject item) => ReleaseItem(item.transform);
        void ReleaseAllItemsOfType<T>() where T : BaseItemObject;
        void ReleaseAllItems();
        void RemoveItemPool();
    }
    
    public class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly ItemConfigContainer _itemConfigContainer;

        public GridItemFactory(IObjectPoolService objectPoolService, GameConfigContainer gameConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _itemConfigContainer = gameConfigContainer.ItemConfigContainer;
        }
        
        public T GetItem<T>(GridObjectTypeData typeData) where T : BaseItemObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetItem();

            var configData = _itemConfigContainer.GetConfigData(typeData);
            
            itemObject.Initialize(typeData)
                      .ApplyData(configData);
            
            return itemObject;
        }
        
        public ItemObject GetRandomItem()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            var objTypeData = new GridObjectTypeData(GridItemKind.Regular, (int)randomType);
            return GetItem<ItemObject>(objTypeData);
        }

        public ObstacleObject GetRandomObstacle()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            var objTypeData = new GridObjectTypeData(GridItemKind.Obstacle, (int)randomType);
            return GetItem<ObstacleObject>(objTypeData);
        }

        public BoosterObject GetRandomBooster()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            var objTypeData = new GridObjectTypeData(GridItemKind.Booster, (int)randomType);
            return GetItem<BoosterObject>(objTypeData);
        }

        public void ReleaseItem(BaseItemObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItemsOfType<T>() where T : BaseItemObject => _objectPoolService.ReturnAllObjectsOfType<T>();
        public void ReleaseAllItems() => _objectPoolService.ReturnAllObjectsOfType<BaseItemObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<BaseItemObject>();
    }
}