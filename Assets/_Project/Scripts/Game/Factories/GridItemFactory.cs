using Core.Config;
using Core.Configs;
using Core.Pool;
using Core.Utils;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectType ıd) where T : BaseGridObject;
        T GetItem<T>(GridItemKind itemKind, int typeId) where T : BaseGridObject => GetItem<T>(new GridObjectType(itemKind, typeId));
        GridObject GetRandomItem();
        ObstacleObject GetRandomObstacle();
        BoosterObject GetRandomBooster();
        void ReleaseItem(BaseGridObject grid);
        void ReleaseItem(Transform item);
        void ReleaseItem(GameObject item) => ReleaseItem(item.transform);
        void ReleaseAllItemsOfType<T>() where T : BaseGridObject;
        void ReleaseAllItems();
        void RemoveItemPool();
    }
    
    public class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly ItemConfigContainer _itemConfigContainer;

        public GridItemFactory(IObjectPoolService objectPoolService, GameplayConfigContainer gameplayConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _itemConfigContainer = gameplayConfigContainer.ItemConfigContainer;
        }
        
        public T GetItem<T>(GridObjectType ıd) where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetItem();

            var configData = _itemConfigContainer.GetConfigData(ıd);
            
            itemObject.Initialize(ıd)
                      .ApplyData(configData);
            
            return itemObject;
        }
        
        public GridObject GetRandomItem()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Regular, (int)randomType);
            return GetItem<GridObject>(objTypeData);
        }

        public ObstacleObject GetRandomObstacle()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ObstacleType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Obstacle, (int)randomType);
            return GetItem<ObstacleObject>(objTypeData);
        }

        public BoosterObject GetRandomBooster()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<BoosterType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Booster, (int)randomType);
            return GetItem<BoosterObject>(objTypeData);
        }

        public void ReleaseItem(BaseGridObject grid) => _objectPoolService.ReturnObject(grid);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItemsOfType<T>() where T : BaseGridObject => _objectPoolService.ReturnAllObjectsOfType<T>();
        public void ReleaseAllItems() => _objectPoolService.ReturnAllObjectsOfType<BaseGridObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<BaseGridObject>();
    }
}