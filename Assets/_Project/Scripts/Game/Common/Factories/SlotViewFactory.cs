using Core.Pool.Services;
using Game.Views;
using UnityEngine;

namespace Game.View.Factories
{
    public sealed class SlotViewFactory : ISlotViewFactory
    {
        private readonly IObjectPoolService _objectPoolService;

        public SlotViewFactory(IObjectPoolService objectPoolService) => _objectPoolService = objectPoolService;

        public T GetSlot<T>() where T : BaseSlotView => _objectPoolService.GetObject<T>();
        public void ReleaseSlot(BaseSlotView grid) => _objectPoolService.ReturnObject(grid);
        public void ReleaseSlot(Transform item) => _objectPoolService.ReturnObject(item);
        public void ReleaseSlotsByType<T>() where T : BaseSlotView => _objectPoolService.ReturnObjectsByType<T>();
        public void RemoveSlotPoolByType<T>() where T : BaseSlotView => _objectPoolService.RemovePoolsByType<T>();
    }
}