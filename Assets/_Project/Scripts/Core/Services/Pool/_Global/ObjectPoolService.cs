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
        
        public T GetObject<T>(T prefab, bool show = true, int poolCount = 1) where T : Component
        {
            var key = prefab.GetInstanceID();
            
            if (_idPoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                var pooledObject = objectPoolHandler.GetObject<T>(show);
                
                return pooledObject;
            }
            else
            {
                objectPoolHandler = CreateNewHandler(prefab, poolCount);
                
                var pooledObject = objectPoolHandler.GetObject<T>(show);

                return pooledObject;
            }
        }

        public T GetObject<T>(bool show = true) where T : Component, IPooledObject
        {
            var key = typeof(T);

            if (_typePoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                var pooledObject = objectPoolHandler.GetObject<T>(show);
                
                return pooledObject;
            }
            else
            {
                objectPoolHandler = CreateNewHandler<T>();
                
                var pooledObject = objectPoolHandler.GetObject<T>(show);

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

        public void RemovePool<T>() where T : IPooledObject
        {
            var key = typeof(T);

            if (!_typePoolHandlers.TryGetValue(key, out var objectPoolHandler))
            {
                EditorLogger.LogError($"You can not remove pool because {key} does not exist in the list of prefabs.");
                return;
            }

            objectPoolHandler.ClearAll<T>();
            
            _typePoolHandlers.Remove(key);
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
                
                var objectPoolHandler = new ObjectPoolHandler();
                
                objectPoolHandler.Initialize(poolAssetConfig.poolObject, _pooledObjectsParent, poolAssetConfig.GetSize(), poolAssetConfig.isLazy);
                
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
        
        private ObjectPoolHandler CreateNewHandler<T>(T obj, int poolCount) where T : Component
        {
            var objectPoolHandler = new ObjectPoolHandler();
            
            var pooledObject = obj.GetOrAddComponent<PooledObject>();
            
            pooledObject.PoolKey = obj.GetInstanceID();

            objectPoolHandler.Initialize(obj.gameObject, _pooledObjectsParent, poolCount).CreatePool();
                        
            _idPoolHandlers.Add(pooledObject.PoolKey, objectPoolHandler);
            
            return objectPoolHandler;
        }

        private ObjectPoolHandler CreateNewHandler<T>()
        {
            var objectPoolHandler = new ObjectPoolHandler();
            
            var type = typeof(T);
        
            var poolData = _poolServiceConfig.poolDataConfigs;
        
            var poolAssetConfig = poolData.FirstOrDefault(x => x.poolObject.GetComponent<T>() != null);

            if (!poolAssetConfig)
            {
                EditorLogger.LogError("Required component not found!");
                return null;
            }
        
            objectPoolHandler.Initialize(poolAssetConfig.poolObject, _pooledObjectsParent, poolAssetConfig.GetSize(), poolAssetConfig.isLazy, poolAssetConfig.isUnique)
                             .CreatePool();
            
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