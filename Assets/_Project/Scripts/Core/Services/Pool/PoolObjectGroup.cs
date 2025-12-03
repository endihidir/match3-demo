using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
using Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Pool
{
    public sealed class PoolObjectGroup
    {
        private GameObject _prefab;
        
        private Transform _pooledObjectRoot;
        
        private int _poolCount;

        private bool _isUnique;
        
        private int _poolKey;
        
        private GameObject _poolParent;
        public Queue<IPooledObject> Pool { get; } = new();
        public bool IsLazy { get; private set; }

        public PoolObjectGroup Initialize(GameObject prefab, Transform rootParent, int poolCount, bool isLazy = true, bool isUnique = false)
        {
            _prefab = prefab;
             
            _poolKey = _prefab.GetInstanceID();
            
            _pooledObjectRoot = rootParent;
            
            _poolCount = poolCount;
            
            IsLazy = isLazy;

            _isUnique = isUnique;
            
            CreatePoolParent();

            return this;
        }

        public PoolObjectGroup CreatePool()
        {
            for (int i = 0; i < _poolCount; i++)
            {
                CreateNewObject(true);
            }

            return this;
        }

        public T GetObject<T>(bool activate = true, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component
        {
            if (HasAnyPooledMissing()) ClearPool();
            
            if (!_poolParent) CreatePoolParent();
            
            IPooledObject pooledObject;
            
            T component = null;
            
            if (_isUnique)
            {
                if (!Pool.TryPeek(out pooledObject))
                {
                    pooledObject = GetNewPooledObject();
                }
            }
            else
            {
                if (!Pool.TryDequeue(out pooledObject))
                {
                    pooledObject = GetNewPooledObject();
                }
            }
            
            if (activate) 
                pooledObject?.Activate(duration, delay, onComplete);


            switch (pooledObject)
            {
                case T tComp:
                    component = tComp;
                    break;
                case Component comp:
                    component = comp.GetComponent<T>();
                    EditorLogger.LogWarning($"[{GetType().Name}] Expected component '{typeof(T).Name}' not found on pooled object '{comp.gameObject.name}'.", component);
                    break;
                default:
                    EditorLogger.LogError($"[{GetType().Name}] IPooledObject is not a Component! Object: {pooledObject}");
                    break;
            }
            
            return component;
        }

        public void ReturnObject<T>(T pooledObject, float duration, float delay, Action onComplete) where T : IPooledObject
        {
            if (!pooledObject.IsActive) return;

            pooledObject.Deactivate(duration, delay, ()=> ReturnToPool(pooledObject, onComplete));
        }

        private void ClearPool()
        {
            foreach (var pooledObject in Pool)
            {
                if (pooledObject is Component pooledObj)
                {
                    Object.Destroy(pooledObj.gameObject);
                }
            }

            Pool?.Clear();

            if (!_poolParent) return;
                
            Object.Destroy(_poolParent);
        }

        public void ClearAll<T>() where T : IPooledObject
        {
            ClearPool();

            foreach (var pooledObject in PoolSearchUtils.FindPooledObjectsOfType<T>())
            {
                if (pooledObject is Component pooledObj)
                {
                    Object.Destroy(pooledObj.gameObject);
                }
                else
                {
                    EditorLogger.LogError($"{pooledObject.GetType().Name} is not Component!");
                }
            }
        }

        private IPooledObject GetNewPooledObject()
        {
            CreateNewObject(false);
            
            return Pool.Dequeue();
        }

        private void CreateNewObject(bool onInitialize)
        {
            var pooledObject = Object.Instantiate(_prefab, _poolParent.transform);

            pooledObject.name = _prefab.name;
            
            var obj = pooledObject.GetOrAddComponent<PooledObject>();

            obj.PoolKey = _poolKey;
            
            if (onInitialize)
            {
                obj.Deactivate();
            }

            Pool.Enqueue(obj);
        }

        private void ReturnToPool(IPooledObject pooledObject, Action onComplete)
        {
            if(!_poolParent) CreatePoolParent();

            if (pooledObject is not Component pooledObj)
            {
                EditorLogger.LogError($"{pooledObject.GetType().Name} is not Component!");
                return;
            }
            
            var pooledObjectT = pooledObj.transform;
            
            pooledObjectT.SetParent(_poolParent.transform);

            pooledObjectT.localPosition = Vector3.zero;
            
            Pool.Enqueue(pooledObject);

            onComplete?.Invoke();
        }
        
        private bool HasAnyPooledMissing() => Pool.Any(pooledObject => pooledObject is null);

        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _prefab.name);
                
            _poolParent.transform.SetParent(_pooledObjectRoot);
        }

        public void Dispose()
        {
            ClearPool();
            
            _prefab = null;
            
            _pooledObjectRoot = null;
            
            _poolParent = null;
            
            _poolCount = 0;
        }
    }
}