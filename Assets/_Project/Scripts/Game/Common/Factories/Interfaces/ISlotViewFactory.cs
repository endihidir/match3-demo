using Game.Views;
using UnityEngine;

namespace Game.View.Factories
{
    public interface ISlotViewFactory
    {
        T GetSlot<T>() where T : BaseSlotView;
        void ReleaseSlot(BaseSlotView grid);
        void ReleaseSlot(Transform item);
        void ReleaseSlotsByType<T>() where T : BaseSlotView;
        void RemoveSlotPoolByType<T>() where T : BaseSlotView;
    }
}