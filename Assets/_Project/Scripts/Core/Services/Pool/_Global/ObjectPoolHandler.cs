using System.Collections.Generic;
using Core.Extensions;
using Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Pool
{
    public sealed class ObjectPoolHandler
    {
        private GameObject _prefab;
        private Transform _pooledObjectRoot;
        private int _poolCount;
        private bool _isUnique;
        private GameObject _poolParent;
        private Queue<IPooledObject> Pool { get; } = new();
        public bool IsLazy { get; private set; }

        public ObjectPoolHandler(GameObject prefab, Transform rootParent, int poolCount, bool isLazy = true, bool isUnique = false)
        {
            _prefab = prefab;
            _pooledObjectRoot = rootParent;
            _poolCount = poolCount;
            IsLazy = isLazy;
            _isUnique = isUnique;
            CreatePoolParent();
        }

        public ObjectPoolHandler CreatePool()
        {
            for (int i = 0; i < _poolCount; i++)
            {
                CreateNewObject(true);
            }

            return this;
        }

        public T GetObject<T>(bool activate = true) where T : Component
        {
            if (HasAnyPooledMissing()) ClearPool();
            
            if (!_poolParent) CreatePoolParent();
            
            IPooledObject pooledObject = null;
            
            T component = null;
            
            if (_isUnique)
            {
                if (Pool.TryPeek(out var peeked))
                {
                    if (peeked is Component c && !c)
                    {
                        ClearPool();
                    }
                    else
                    {
                        pooledObject = peeked;
                    }
                }
            }
            else
            {
                while (Pool.TryDequeue(out var candidate))
                {
                    if (candidate is Component c && !c) continue;

                    pooledObject = candidate;
                    break;
                }
            }
            
            pooledObject ??= GetNewPooledObject();
            
            if (activate) 
                pooledObject?.Activate();


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

        public void ReturnObject<T>(T pooledObject, bool hide = true) where T : IPooledObject
        {
            if (pooledObject.IsActive && hide)
            {
                pooledObject.Deactivate();
            }
            
            ReturnToPool(pooledObject);
        }

        private IPooledObject GetNewPooledObject()
        {
            CreateNewObject(false);
            
            return Pool.Dequeue();
        }

        private void CreateNewObject(bool onInitialize)
        {
            var objClone = Object.Instantiate(_prefab, _poolParent.transform);

            objClone.name = _prefab.name;
            
            var pooledObject = objClone.GetOrAddComponent<PooledObject>();

            pooledObject.PoolKey = _prefab.GetInstanceID();
            
            if (onInitialize)
            {
                pooledObject.Deactivate();
            }

            Pool.Enqueue(pooledObject);
        }

        private void ReturnToPool(IPooledObject pooledObject)
        {
            if(!_poolParent) CreatePoolParent();

            if (pooledObject is not Component pooledObj)
            {
                EditorLogger.LogError($"{pooledObject.GetType().Name} is not Component!");
                return;
            }
            
            pooledObj.transform.SetParent(_poolParent.transform);

            pooledObj.transform.localPosition = Vector3.zero;
            
            Pool.Enqueue(pooledObject);
        }
        
        private bool HasAnyPooledMissing()
        {
            foreach (var po in Pool)
            {
                if (po == null) return true;
                if (po is Component c && !c) return true;
            }
            return false;
        }

        private void CreatePoolParent()
        {
            _poolParent = new GameObject("Pool_" + _prefab.name);
            _poolParent.transform.SetParent(_pooledObjectRoot);
        }
        
        public void ClearPool()
        {
            foreach (var pooledObject in Pool)
            {
                if (pooledObject is not Component pooledObj) continue;
                if (!pooledObj) continue;
                Object.Destroy(pooledObj.gameObject);
            }

            Pool?.Clear();
            
            if (!_poolParent) return;
            Object.Destroy(_poolParent);
            
            _prefab = null;
            _pooledObjectRoot = null;
            _poolCount = 0;
        }

        public void Dispose() => ClearPool();
    }
}