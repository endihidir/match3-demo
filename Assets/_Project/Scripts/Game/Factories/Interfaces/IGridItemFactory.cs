using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectType type) where T : BaseGridObject;
        void ReleaseItem(BaseGridObject item);
        void ReleaseItem(Transform item);
        void ReleaseItemsByType<T>() where T : BaseGridObject;
        void RemovePoolsByType<T>() where T : BaseGridObject;
    }
}