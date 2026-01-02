using Core.Config;
using Core.Configs;
using Core.Pool;
using Core.Utils;
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
        void ReleaseAllItems();
        void RemoveItemPool();
    }
    
    public class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly ItemConfigContainerSO _ıtemConfigContainerSo;

        public GridItemFactory(IObjectPoolService objectPoolService, GameplayConfigContainer gameplayConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _ıtemConfigContainerSo = gameplayConfigContainer.ItemConfigContainer;
        }
        
        public T GetItem<T>(GridObjectType typeData) where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetItem();

            var configData = _ıtemConfigContainerSo.GetConfigData(typeData);
            
            itemObject.Initialize(typeData)
                      .ApplyData(configData);
            
            return itemObject;
        }

        public ItemObject GetRandomItem()
        {
            var randomType = LevelGridRandomUtil.GetRandomEnumValue<ItemType>(1);
            var objTypeData = new GridObjectType(GridItemKind.Regular, (int)randomType);
            return GetItem<ItemObject>(objTypeData);
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