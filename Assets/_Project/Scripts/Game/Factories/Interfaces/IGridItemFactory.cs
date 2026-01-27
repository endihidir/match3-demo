using UnityEngine;

namespace Core.Item.Factories
{
    public interface IGridItemFactory
    {
        T GetItem<T>(GridObjectType typeData) where T : BaseGridObject;
        void ReleaseItem(BaseGridObject grid);
        void ReleaseItem(Transform item);
        void ReleaseAllItemsOfType<T>() where T : BaseGridObject;
        void ReleaseAll();
        void Remove();
    }
}