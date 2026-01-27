using Core.UI;
using UnityEngine;

namespace Core.Item.Factories
{
    public interface ISlotViewFactory
    {
        T GetItem<T>() where T : BaseSlotView;
        void ReleaseItem(BaseSlotView grid);
        void ReleaseItem(Transform item);
        void ReleaseAllItemsOfType<T>() where T : BaseSlotView;
        void ReleaseAll();
        void Remove();
    }
}