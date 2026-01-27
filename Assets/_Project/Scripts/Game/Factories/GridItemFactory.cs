using Core.Config;
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
        
        public T GetItem<T>(GridObjectType typeData) where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            
            itemObject.ResetItem();

            var configData = _gridConfigContainer.GetConfigData(typeData);
            
            itemObject.Initialize(typeData)
                      .ApplyData(configData);
            
            return itemObject;
        }
        
        public void ReleaseItem(BaseGridObject grid) => _objectPoolService.ReturnObject(grid);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItemsOfType<T>() where T : BaseGridObject => _objectPoolService.ReturnAllObjectsOfType<T>();
        public void ReleaseAll() => _objectPoolService.ReturnAllObjectsOfType<BaseGridObject>();
        public void Remove() => _objectPoolService.RemovePoolOfType<BaseGridObject>();
    }
}