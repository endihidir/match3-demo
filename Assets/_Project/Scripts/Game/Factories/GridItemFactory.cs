using Core.Configs;
using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public class GridItemFactory : IGridItemFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        private readonly GridConfigContainerSO _gridConfigContainer;

        public GridItemFactory(IObjectPoolService objectPoolService, GameplayConfigContainer gameplayConfigContainer)
        {
            _objectPoolService = objectPoolService;
            _gridConfigContainer = gameplayConfigContainer.GridConfigContainer;
        }
        
        public T GetItem<T>(GridObjectType type) where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetItem();

            var configData = _gridConfigContainer.GetConfigData(type);
            
            itemObject.Initialize(type)
                      .ApplyData(configData);
            
            return itemObject;
        }
        
        public void ReleaseItem(BaseGridObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseItemsByType<T>() where T : BaseGridObject => _objectPoolService.ReturnObjectsByType<T>();
        public void RemovePoolsByType<T>() where T : BaseGridObject => _objectPoolService.RemovePoolsByType<T>();
    }
}