using System;
using System.Collections.Generic;
using System.Linq;
using Core.Configs;
using Core.Extensions;
using Core.Utils;
using UnityEngine;

namespace Core.Pool
{
    public class ObjectPoolService : IObjectPoolService, IDisposable
    {
        private const string ROOT_NAME = "PooledObjectsHolder";
        
        private readonly PoolServiceConfig _poolServiceConfig;
        private readonly IDictionary<int, ObjectPoolHandler> _idPoolHandlers = new Dictionary<int, ObjectPoolHandler>();
        private readonly IDictionary<Type, ObjectPoolHandler> _typePoolHandlers = new Dictionary<Type, ObjectPoolHandler>();
        
        private Transform _pooledObjectsParent;
        public ObjectPoolService(AppConfigContainer appConfigContainer) => _poolServiceConfig = appConfigContainer.poolServiceConfig;

        public void Initialize()
        {
            var root = GameObject.Find(ROOT_NAME) ?? new GameObject(ROOT_NAME);
            _pooledObjectsParent = root.transform;    
            CacheAllPooledObjects();
            CreateAllCachedPooledObjects();
        }
        
        public T GetObject<T>(T prefab, bool show = true, int poolCount = 1, bool isLazy = true, bool showLogs = false) where T : Component
        {
            var key = prefab.gameObject.GetInstanceID();
            
            if (_idPoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                var pooledObject = objectPoolHandler.GetObject<T>(show, showLogs);
                
                return pooledObject;
            }
            else
            {
                objectPoolHandler = CreateNewHandler(prefab, poolCount, isLazy);
                
                var pooledObject = objectPoolHandler.GetObject<T>(show, showLogs);

                return pooledObject;
            }
        }

        public T GetObject<T>(bool show = true, bool showLogs = false) where T : Component, IPooledObject
        {
            var key = typeof(T);

            if (_typePoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                var pooledObject = objectPoolHandler.GetObject<T>(show, showLogs);
                
                return pooledObject;
            }
            else
            {
                objectPoolHandler = CreateNewHandler<T>();
                
                var pooledObject = objectPoolHandler.GetObject<T>(show, showLogs);

                return pooledObject;
            }
        }

        public void ReturnObject<T>(T objectRef, bool hide = true) where T : Component
        {
            if (!objectRef) { EditorLogger.LogError($"[{GetType().Name}] Return failed: null/destroyed object"); return; }

            if (!objectRef.TryGetComponent<IPooledObject>(out var pooledObject))
            {
                EditorLogger.LogError($"[{GetType().Name}] Return failed: object is not IPooledObject.");
                return;
            }

            if (pooledObject == null) return;

            var idKey = pooledObject.PoolKey;
            var typeKey = pooledObject.GetType();
            
            if (!_idPoolHandlers.TryGetValue(idKey, out var objectPoolHandler))
            {
                if (!_typePoolHandlers.TryGetValue(typeKey, out objectPoolHandler))
                {
                    EditorLogger.LogError($"[{GetType().Name}] Return failed: no handler for '{objectRef.name}' ({objectRef.GetType().Name}).");
                    return;
                }
            }
            
            objectPoolHandler.ReturnObject(pooledObject, hide);
        }
        
        public void ReturnObjectsByType<T>(bool hide = true) where T : Component, IPooledObject
        {
            var pooledObjects = PoolSearchUtils.FindPooledObjectsOfType<T>();
            
            foreach (var pooledObject in pooledObjects)
            {
                ReturnObject(pooledObject, hide);
            }
        }
        
        public void ReturnAll(bool hide = true)
        {
            var pooledObjects = PoolSearchUtils.FindPooledObjectsOfType<IPooledObject>();
            
            foreach (var pooledObject in pooledObjects)
            {
                if (pooledObject is Component component)
                {
                    ReturnObject(component, hide);
                }
            }
        }
        
        public void RemovePoolsByType<T>() where T : IPooledObject
        {
            var baseType = typeof(T);

            var keysToRemove = new List<Type>();

            foreach (var kvp in _typePoolHandlers)
            {
                if (!baseType.IsAssignableFrom(kvp.Key)) continue;
                
                kvp.Value.Dispose();
                
                keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _typePoolHandlers.Remove(key);
            }
        }

        public void RemovePoolsByPrefab<T>(T prefab) where T : Component, IPooledObject
        {
            var id = prefab.gameObject.GetInstanceID();

            var keysToRemove = new List<int>();

            foreach (var kvp in _idPoolHandlers)
            {
                if (id != kvp.Key) continue;
                
                kvp.Value.Dispose();
                
                keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _idPoolHandlers.Remove(key);
            }
        }
        
        public void RemoveAllPools()
        {
            foreach (var kvp in _typePoolHandlers)
            {
                kvp.Value?.Dispose();
            }

            _typePoolHandlers.Clear();

            foreach (var kvp in _idPoolHandlers)
            {
                kvp.Value?.Dispose();
            }

            _idPoolHandlers.Clear();
        }

        private void CacheAllPooledObjects()
        {
            var poolData = _poolServiceConfig.poolDataConfigs;

            foreach (var poolAssetConfig in poolData.Distinct())
            {
                if (!poolAssetConfig.PoolObject)
                {
                    EditorLogger.LogError("There is missing prefab in pool object list!");
                    continue;
                }

                var isPooledObject = poolAssetConfig.PoolObject.TryGetComponent<IPooledObject>(out var pooledObjects);
                if(!isPooledObject) continue;
                var key = pooledObjects.GetType();
                
                if (_typePoolHandlers.ContainsKey(key)) continue;
                var size = poolAssetConfig.PoolSize;
                var isLazy = poolAssetConfig.IsLazy;
                
                var objectPoolHandler = new ObjectPoolHandler(poolAssetConfig.PoolObject, _pooledObjectsParent, size, isLazy);
                _typePoolHandlers.Add(key, objectPoolHandler);
            }
        }
        
        private void CreateAllCachedPooledObjects()
        {
            var nonLazyPooledObjects = _typePoolHandlers.Where(poolData => !poolData.Value.IsLazy).ToDictionary(x=> x.Key,y => y.Value);

            foreach (var keyValuePair in nonLazyPooledObjects)
            {
               keyValuePair.Value?.CreatePool(); 
            }
        }
        
        private ObjectPoolHandler CreateNewHandler<T>(T prefab, int poolCount, bool isLazy = true) where T : Component
        {
            prefab.GetOrAddComponent<PooledObject>();
            var prefabObj = prefab.gameObject;
            var objectPoolHandler = new ObjectPoolHandler(prefabObj, _pooledObjectsParent, poolCount, isLazy).CreatePool();
            var key = prefabObj.GetInstanceID();
            _idPoolHandlers.Add(key, objectPoolHandler);
            return objectPoolHandler;
        }

        private ObjectPoolHandler CreateNewHandler<T>() where T : Component, IPooledObject
        {
            var type = typeof(T);
            var poolData = _poolServiceConfig.poolDataConfigs;
            var poolAssetConfig = poolData.FirstOrDefault(x => x.PoolObject.GetComponent<T>());

            if (!poolAssetConfig)
            {
                EditorLogger.LogError("Required component not found!");
                return null;
            }

            var size = poolAssetConfig.PoolSize;
            var isLazy = poolAssetConfig.IsLazy;
            var objectPoolHandler = new ObjectPoolHandler(poolAssetConfig.PoolObject, _pooledObjectsParent, size, isLazy).CreatePool();
            _typePoolHandlers.Add(type, objectPoolHandler);
            return objectPoolHandler;
        }
        
        public void Dispose() => RemoveAllPools();
    }
}