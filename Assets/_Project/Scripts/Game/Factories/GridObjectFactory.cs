using Core.Pool.Services;
using UnityEngine;

namespace Game.Grid.Item.Factories
{
    public sealed class GridObjectFactory : IGridObjectFactory
    {
        private readonly IObjectPoolService _objectPoolService;
        public GridObjectFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetObject<T>(bool activate = true) where T : BaseGridObject
        {
            var itemObject = _objectPoolService.GetObject<T>(activate);
            itemObject.ResetItem();
            return itemObject;
        }
        
        public void ReleaseObject(BaseGridObject item) => _objectPoolService.ReturnObject(item);
        public void ReleaseObject(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseObjectsByType<T>() where T : BaseGridObject => _objectPoolService.ReturnObjectsByType<T>();
        public void RemovePoolsByType<T>() where T : BaseGridObject => _objectPoolService.RemovePoolsByType<T>();
    }
}