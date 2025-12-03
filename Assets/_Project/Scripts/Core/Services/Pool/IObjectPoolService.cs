using System;
using UnityEngine;

namespace Core.Pool
{
    public interface IObjectPoolService : IReturnToPool, IDisposable
    {
        void Initialize();
        T GetObject<T>(T prefab, bool show = true, int poolCount = 1, Action onComplete = null) where T : Component;
        T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component, IPooledObject;
        void RemovePool<T>() where T : IPooledObject;
    }
    
    public interface IReturnToPool
    {
        void ReturnObject<T>(T objectRef, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component;
        void ReturnAllObjectsOfType<T>(float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component, IPooledObject;
        void ReturnAll(float duration, float delay, Action onComplete = null);
    }
}