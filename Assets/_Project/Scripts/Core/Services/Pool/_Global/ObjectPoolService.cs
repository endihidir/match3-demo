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
        private const string ROOT_NAME = "PooledObjectHolder";
        
        private readonly PoolServiceConfig _poolServiceConfig;
        
        private Transform _pooledObjectsParent;
        
        private readonly IDictionary<int, ObjectPoolHandler> _idPoolHandlers = new Dictionary<int, ObjectPoolHandler>();
        
        private IDictionary<Type, ObjectPoolHandler> _typePoolHandlers = new Dictionary<Type, ObjectPoolHandler>();
        public ObjectPoolService(AppConfigContainer gameDataHolderSo) => _poolServiceConfig = gameDataHolderSo.poolServiceConfig;

        public void Initialize()
        {
            var root = GameObject.Find(ROOT_NAME) ?? new GameObject(ROOT_NAME);
            
            _pooledObjectsParent = root.transform;    
            
            CacheAllPooledObjects();
            
            CreateAllCachedPooledObjects();
        }
        
        public T GetObject<T>(T prefab, bool show = true, int poolCount = 1, bool isLazy = true, bool isUnique = false, bool showLogs = false) where T : Component
        {
            var key = prefab.gameObject.GetInstanceID();
            
            if (_idPoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                var pooledObject = objectPoolHandler.GetObject<T>(show, showLogs);
                
                return pooledObject;
            }
            else
            {
                objectPoolHandler = CreateNewHandler(prefab, poolCount, isLazy, isUnique);
                
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
        
        public void ReturnAllObjectsOfType<T>(bool hide = true) where T : Component, IPooledObject
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

        public void RemovePoolOfType<T>() where T : IPooledObject
        {
            var key = typeof(T);

            if (!_typePoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                EditorLogger.LogError($"You can not remove pool because {key} does not exist in the list of prefabs.");
                return;
            }

            objectPoolHandler.ClearPool();
            
            _typePoolHandlers.Remove(key);
        }
        
        public void RemovePool<T>(T prefab) where T : Component
        {
            var key = prefab.gameObject.GetInstanceID();

            if (!_idPoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                EditorLogger.LogError($"You can not remove pool because {key} does not exist in the list of prefabs.");
                return;
            }

            objectPoolHandler.ClearPool();
            
            _idPoolHandlers.Remove(key);
        }

        private void CacheAllPooledObjects()
        {
            var poolData = _poolServiceConfig.poolDataConfigs;

            foreach (var poolAssetConfig in poolData.Distinct())
            {
                if (!poolAssetConfig.poolObject)
                {
                    EditorLogger.LogError("There is missing prefab in pool object list!");
                    continue;
                }

                var isPooledObject = poolAssetConfig.poolObject.TryGetComponent<IPooledObject>(out var pooledObjects);
                
                if(!isPooledObject) continue;
                
                var key = pooledObjects.GetType();
                
                if (_typePoolHandlers.ContainsKey(key)) continue;
                
                var size = poolAssetConfig.GetSize();
                var isLazy = poolAssetConfig.isLazy;
                var isUnique = poolAssetConfig.isUnique;
                
                var objectPoolHandler = new ObjectPoolHandler(poolAssetConfig.poolObject, _pooledObjectsParent, size, isLazy, isUnique);
                
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
        
        private ObjectPoolHandler CreateNewHandler<T>(T prefab, int poolCount, bool isLazy = true, bool isUnique = false) where T : Component
        {
            prefab.GetOrAddComponent<PooledObject>();
            
            var objectPoolHandler = new ObjectPoolHandler(prefab.gameObject, _pooledObjectsParent, poolCount, isLazy, isUnique).CreatePool();

            var key = prefab.gameObject.GetInstanceID();
            
            _idPoolHandlers.Add(key, objectPoolHandler);
            
            return objectPoolHandler;
        }

        private ObjectPoolHandler CreateNewHandler<T>()
        {
            var type = typeof(T);
        
            var poolData = _poolServiceConfig.poolDataConfigs;
        
            var poolAssetConfig = poolData.FirstOrDefault(x => x.poolObject.GetComponent<T>() != null);

            if (!poolAssetConfig)
            {
                EditorLogger.LogError("Required component not found!");
                return null;
            }

            var size = poolAssetConfig.GetSize();
            var isLazy = poolAssetConfig.isLazy;
            var isUnique = poolAssetConfig.isUnique;
        
            var objectPoolHandler = new ObjectPoolHandler(poolAssetConfig.poolObject, _pooledObjectsParent, size, isLazy, isUnique).CreatePool();
            
            _typePoolHandlers.Add(type, objectPoolHandler);
            
            return objectPoolHandler;
        }
        
        public void Dispose()
        {
            foreach (var keyValuePair in _typePoolHandlers)
            {
                keyValuePair.Value?.Dispose();
            }
            
            foreach (var keyValuePair in _idPoolHandlers)
            {
                keyValuePair.Value?.Dispose();
            }
            
            _idPoolHandlers?.Clear();
            _typePoolHandlers?.Clear();
        }
    }
}