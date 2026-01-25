using UnityEngine;

namespace Core.Pool
{
    public interface IObjectPoolService
    {
        void Initialize();
        T GetObject<T>(T prefab, bool show = true, int poolCount = 1, bool isLazy = true, bool isUnique = false) where T : Component;
        T GetObject<T>(bool show = true) where T : Component, IPooledObject;
        
        void ReturnObject<T>(T objectRef, bool hide = true) where T : Component;
        void ReturnAllObjectsOfType<T>(bool hide = true) where T : Component, IPooledObject;
        void ReturnAll(bool hide = true);
        
        void RemovePool<T>(T prefab) where T : Component;
        void RemovePoolOfType<T>() where T : IPooledObject;
    }
}