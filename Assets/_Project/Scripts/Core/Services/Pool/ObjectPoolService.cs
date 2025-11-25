using System;
using System.Collections.Generic;
using System.Linq;
using Core.Configs;
using Core.Extensions;
using Core.Utils;
using UnityEngine;

namespace Core.Pool
{
    public class ObjectPoolService : IObjectPoolService
    {
        private const string ROOT_NAME = "PooledObjectHolder";
        
        private readonly PoolServiceConfig _poolServiceConfig;
        
        private Transform _pooledObjectsParent;
        
        private readonly IDictionary<int, PoolObjectGroup> _pooledObjectGroupsWithID = new Dictionary<int, PoolObjectGroup>();
        
        private IDictionary<Type, PoolObjectGroup> _pooledGroupsWithType = new Dictionary<Type, PoolObjectGroup>();
        
        public ObjectPoolService(AppConfigContainer gameDataHolderSo)
        {
            _poolServiceConfig = gameDataHolderSo.poolServiceConfig;
        }

        public void Initialize()
        {
            var root = GameObject.Find(ROOT_NAME) ?? new GameObject(ROOT_NAME);
            
            _pooledObjectsParent = root.transform;    
            
            CacheAllPooledObjects();
            
            CreateAllCachedPooledObjects();
        }
        
        public T GetObject<T>(T prefab, bool show = true, int poolCount = 1, Action onComplete = null) where T : Component
        {
            var key = prefab.GetInstanceID();
            
            if (_pooledObjectGroupsWithID.TryGetValue(key, out var poolObjectGroup))
            {
                var pooledObject = poolObjectGroup.GetObject<T>(show, 0f, 0f, onComplete);
                
                return pooledObject;
            }
            else
            {
                poolObjectGroup = CreateNewGroup(prefab, poolCount);
                
                var pooledObject = poolObjectGroup.GetObject<T>(show, 0f, 0f, onComplete);

                return pooledObject;
            }
        }

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component, IPooledObject
        {
            var key = typeof(T);

            if (_pooledGroupsWithType.TryGetValue(key, out var poolObjectGroup))
            {
                var pooledObject = poolObjectGroup.GetObject<T>(show, duration, delay, onComplete);
                
                return pooledObject;
            }
            else
            {
                poolObjectGroup = CreateNewGroup<T>();
                
                var pooledObject = poolObjectGroup.GetObject<T>(show, duration, delay, onComplete);

                return pooledObject;
            }
        }

        public void ReturnObject<T>(T objectRef, float duration, float delay, Action onComplete = null) where T : Component
        {
            if (!objectRef) { ConditionalDebug.LogError($"[{GetType()}] Return failed: null/destroyed object"); return; }

            if (!objectRef.TryGetComponent<IPooledObject>(out var pooledObject))
            {
                ConditionalDebug.LogError($"[{GetType()}] Return failed: object is not IPooledObject.");
                return;
            }

            if (!_pooledObjectGroupsWithID.TryGetValue(pooledObject.PoolKey, out var group))
            {
                if (!_pooledGroupsWithType.TryGetValue(pooledObject.GetType(), out group))
                {
                    ConditionalDebug.LogError($"[{GetType()}] Return failed: no group for '{objectRef.name}' ({objectRef.GetType().Name}).");
                    return;
                }
            }
            
            group.ReturnObject(pooledObject, duration, delay, onComplete);
        }
        
        public void ReturnAllObjectsOfType<T>(float duration, float delay, Action onComplete = null) where T : Component, IPooledObject
        {
            var pooledObjects = PoolSearchUtils.FindPooledObjectsOfType<T>();
            
            foreach (var pooledObject in pooledObjects)
            {
                ReturnObject(pooledObject, duration, delay, onComplete);
            }
        }

        public void ReturnAll(float duration, float delay, Action onComplete = null)
        {
            var pooledObjects = PoolSearchUtils.FindPooledObjectsOfType<IPooledObject>();
            
            foreach (var pooledObject in pooledObjects)
            {
                if (pooledObject is Component component)
                {
                    ReturnObject(component, duration, delay, onComplete);
                }
            }
        }

        public void RemovePool<T>() where T : IPooledObject
        {
            var key = typeof(T);

            if (!_pooledGroupsWithType.TryGetValue(key, out var poolObjectGroup))
            {
                ConditionalDebug.LogError($"You can not remove pool because {key} does not exist in the list of prefabs.");
                return;
            }

            poolObjectGroup.ClearAll<T>();
            
            _pooledGroupsWithType.Remove(key);
        }

        private void CacheAllPooledObjects()
        {
            var poolData = _poolServiceConfig.poolDataConfigs;

            foreach (var poolAssetConfig in poolData.Distinct())
            {
                if (!poolAssetConfig.poolObject)
                {
                    ConditionalDebug.LogError("There is missing prefab in pool object list!");
                    continue;
                }

                var isPooledObject = poolAssetConfig.poolObject.TryGetComponent<IPooledObject>(out var pooledObjects);
                
                if(!isPooledObject) continue;
                
                var key = pooledObjects.GetType();
                
                if (_pooledGroupsWithType.ContainsKey(key)) continue;
                
                var poolObjectGroup = new PoolObjectGroup();
                
                poolObjectGroup.Initialize(poolAssetConfig.poolObject, _pooledObjectsParent, poolAssetConfig.GetSize(), poolAssetConfig.isLazy);
                
                _pooledGroupsWithType.Add(key, poolObjectGroup);
            }
        }
        
        private void CreateAllCachedPooledObjects()
        {
            var nonLazyPooledObjects = _pooledGroupsWithType.Where(poolData => !poolData.Value.IsLazy).ToDictionary(x=> x.Key,y => y.Value);

            foreach (var keyValuePair in nonLazyPooledObjects)
            {
               keyValuePair.Value?.CreatePool(); 
            }
        }
        
        private PoolObjectGroup CreateNewGroup<T>(T obj, int poolCount) where T : Component
        {
            var poolObjectGroup = new PoolObjectGroup();
            
            var pooledObject = obj.GetOrAddComponent<PooledObject>();
            
            pooledObject.PoolKey = obj.GetInstanceID();

            poolObjectGroup.Initialize(obj.gameObject, _pooledObjectsParent, poolCount).CreatePool();
                        
            _pooledObjectGroupsWithID.Add(pooledObject.PoolKey, poolObjectGroup);
            
            return poolObjectGroup;
        }

        private PoolObjectGroup CreateNewGroup<T>()
        {
            var poolObjectGroup = new PoolObjectGroup();
            
            var type = typeof(T);
        
            var poolData = _poolServiceConfig.poolDataConfigs;
        
            var poolAssetConfig = poolData.FirstOrDefault(x => x.poolObject.GetComponent<T>() != null);

            if (!poolAssetConfig)
            {
                ConditionalDebug.LogError("Required component not found!");
                return null;
            }
        
            poolObjectGroup.Initialize(poolAssetConfig.poolObject, _pooledObjectsParent, poolAssetConfig.GetSize(), poolAssetConfig.isLazy, poolAssetConfig.isUnique)
                .CreatePool();
            
            _pooledGroupsWithType.Add(type, poolObjectGroup);
            
            return poolObjectGroup;
        }
        
        public void Dispose()
        {
            foreach (var keyValuePair in _pooledGroupsWithType)
            {
                keyValuePair.Value?.Dispose();
            }
            
            foreach (var keyValuePair in _pooledObjectGroupsWithID)
            {
                keyValuePair.Value?.Dispose();
            }
           
            _pooledGroupsWithType = null;
        }
    }
    
    public interface IObjectPoolService : IReturnToPool, IDisposable
    {
        public void Initialize();
        public T GetObject<T>(T prefab, bool show = true, int poolCount = 1, Action onComplete = null) where T : Component;
        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component, IPooledObject;
    
        public void ReturnAllObjectsOfType<T>(float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component, IPooledObject;
        public void ReturnAll(float duration, float delay, Action onComplete = null);
        public void RemovePool<T>() where T : IPooledObject;
    }
    
    public interface IReturnToPool
    {
        public void ReturnObject<T>(T objectRef, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component;
    }
}