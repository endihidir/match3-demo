using UnityEngine;

namespace Core.Pool
{
    public interface IObjectPoolService
    {
        void Initialize();
        T GetObject<T>(T prefab, bool show = true, int poolCount = 1, bool isLazy = true, bool showLogs = false) where T : Component;
        T GetObject<T>(bool show = true, bool showLogs = false) where T : Component, IPooledObject;
        
        void ReturnObject<T>(T objectRef, bool hide = true) where T : Component;
        void ReturnObjectsByType<T>(bool hide = true) where T : Component, IPooledObject;
        void ReturnAll(bool hide = true);
        
        void RemovePoolsByType<T>() where T : IPooledObject;
        void RemovePoolsByPrefab<T>(T prefab) where T : Component, IPooledObject;
        void RemoveAllPools();
    }
}