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

        public T GetObject<T>(bool show = true, float duration = 0f, float delay = 0f, Action onComplete = null) where T : Component
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
            
            if (show) 
                pooledObject?.Show(duration, delay, onComplete);


            if (pooledObject is T tComponent)
            {
                component = tComponent;
            }
            else if (pooledObject is Component comp)
            {
                component = comp.GetComponent<T>();
                
                ConditionalDebug.LogWarning($"[PoolManager] Expected component '{typeof(T).Name}' not found on pooled object '{comp.gameObject.name}'.", component);
            }
            else
            {
                ConditionalDebug.LogError($"[PoolManager] IPooledObject is not a Component! Object: {pooledObject}");
            }
            
            
            return component;
        }

        public void HideObject<T>(T pooledObject, float duration, float delay, Action onComplete) where T : IPooledObject
        {
            if (!pooledObject.IsActive) return;

            pooledObject.Hide(duration, delay, ()=> ReturnToPool(pooledObject, onComplete));
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

            foreach (var pooledObject in FindPooledObjectsOfType<T>())
            {
                if (pooledObject is Component pooledObj)
                {
                    Object.Destroy(pooledObj.gameObject);
                }
                else
                {
                    ConditionalDebug.LogError($"{pooledObject.GetType()} is not Component!");
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
                obj.Hide();
            }

            Pool.Enqueue(obj);
        }

        private void ReturnToPool(IPooledObject pooledObject, Action onComplete)
        {
            if(!_poolParent) CreatePoolParent();

            if (pooledObject is not Component pooledObj)
            {
                ConditionalDebug.LogError($"{pooledObject.GetType()} is not Component!");
                return;
            }
            
            var pooledObjectT = pooledObj.transform;
            
            pooledObjectT.SetParent(_poolParent.transform);

            pooledObjectT.localPosition = Vector3.zero;

            pooledObject.Hide();
            
            Pool.Enqueue(pooledObject);

            onComplete?.Invoke();
        }
        
        public static IEnumerable<T> FindPooledObjectsOfType<T>(bool includeInactive = false) where T : IPooledObject
        {
            return Object.FindObjectsOfType<MonoBehaviour>(includeInactive).OfType<T>().Where(pooledObject => pooledObject.IsActive);
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