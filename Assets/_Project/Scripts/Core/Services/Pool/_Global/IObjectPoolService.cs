using System;
using UnityEngine;

namespace Core.Pool
{
    public interface IObjectPoolService : IReturnToPool, IDisposable
    {
        void Initialize();
        T GetObject<T>(T prefab, bool show = true, int poolCount = 1) where T : Component;
        T GetObject<T>(bool show = true) where T : Component, IPooledObject;
        void RemovePool<T>() where T : IPooledObject;
    }
    
    public interface IReturnToPool
    {
        void ReturnObject<T>(T objectRef, bool hide = true) where T : Component;
        void ReturnAllObjectsOfType<T>(bool hide = true) where T : Component, IPooledObject;
        void ReturnAll(bool hide = true);
    }
}