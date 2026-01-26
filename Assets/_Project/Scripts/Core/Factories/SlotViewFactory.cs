using Core.Pool;
using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public class SlotViewFactory : ISlotViewFactory, IFactoryResettable
    {
        private readonly IObjectPoolService _objectPoolService;

        public SlotViewFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetItem<T>() where T : BaseSlotView => _objectPoolService.GetObject<T>();
        
        public void ReleaseItem(BaseGridObject grid) => _objectPoolService.ReturnObject(grid);
        public void ReleaseItem(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseAllItemsOfType<T>() where T : BaseGridObject => _objectPoolService.ReturnAllObjectsOfType<T>();
        public void Reset() => _objectPoolService.ReturnAllObjectsOfType<BaseGridObject>();
        public void Remove() => _objectPoolService.RemovePoolOfType<BaseGridObject>();
    }
}