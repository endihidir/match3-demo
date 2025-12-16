using Core.Config;
using Core.Configs;
using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectTypeData gridObjectTypeData) where T : BaseItemObject;
        T GetItem<T>(GridItemKind itemKind, int typeId) where T : BaseItemObject => GetItem<T>(new GridObjectTypeData(itemKind, typeId));
        BaseItemObject GetItem(ItemType type) => GetItem<ItemObject>(GridItemKind.Regular, (int)type);
        BaseItemObject GetItem(BoosterType type) => GetItem<BoosterObject>(GridItemKind.Booster, (int)type);
        BaseItemObject GetItem(ObstacleType type) => GetItem<ObstacleObject>(GridItemKind.Obstacle, (int)type);
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
        
        public T GetItem<T>(GridObjectTypeData gridObjectTypeData) where T : BaseItemObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetState();

            var configData = _itemConfigContainer.GetConfigData(gridObjectTypeData);
            
            itemObject.Initialize(gridObjectTypeData)
                      .ApplyData(configData);
            
            return itemObject;
        }

        public void ReleaseItem(BaseItemObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItemsOfType<T>() where T : BaseItemObject => _objectPoolService.ReturnAllObjectsOfType<T>();
        public void ReleaseAllItems() => _objectPoolService.ReturnAllObjectsOfType<BaseItemObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<BaseItemObject>();
    }
}