using UnityEngine;

namespace Game.Grid.Item.Factories
{
    public interface IGridObjectFactory
    {
        T GetObject<T>(bool activate = true) where T : BaseGridObject;
        void ReleaseObject(BaseGridObject item);
        void ReleaseObject(Transform item);
        void ReleaseObjectsByType<T>() where T : BaseGridObject;
        void RemovePoolsByType<T>() where T : BaseGridObject;
    }
}