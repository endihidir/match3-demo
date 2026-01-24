using Core.Config;
using Core.Configs;
using Core.Pool;
using Core.Utils;
using UnityEngine;

namespace Core.Item.Factories
{
    public class GridItemFactory : IGridItemFactory, IFactoryCleaner
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly GridConfigContainerSO _gridConfigContainer;

        public GridItemFactory(IObjectPoolService objectPoolService, GameplayConfigContainer gameplayConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _gridConfigContainer = gameplayConfigContainer.GridConfigContainer;
        }
        
        public T GetItem<T>(GridObjectType typeData) where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetItem();

            var configData = _gridConfigContainer.GetConfigData(typeData);
            
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
        public void CleanupFactory() => _objectPoolService.ReturnAllObjectsOfType<BaseGridObject>();
        public void RemoveItemPool() => _objectPoolService.RemovePool<BaseGridObject>();
    }
}