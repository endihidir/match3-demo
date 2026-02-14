using Core.Pool;
using UnityEngine;

namespace Core.Item.Factories
{
    public class GridObjectFactory : IGridObjectFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        public GridObjectFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetObject<T>() where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>();
            itemObject.ResetItem();
            return itemObject;
        }
        
        public void ReleaseObject(BaseGridObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseObject(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseObjectsByType<T>() where T : BaseGridObject => _objectPoolService.ReturnObjectsByType<T>();
        public void RemovePoolsByType<T>() where T : BaseGridObject => _objectPoolService.RemovePoolsByType<T>();
    }
}