using UnityEngine;

namespace Core.Pool
{
    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour, IPooledObject
    {
        public int PoolKey { get; set; }
        public virtual bool IsActive => gameObject.activeInHierarchy;

        public virtual void Activate()
        {
            gameObject.SetActive(true);
            OnSpawned();
        }

        public virtual void Deactivate()
        {
            gameObject.SetActive(false);
            OnDespawned();
        }
        
        protected virtual void OnSpawned() { }
        protected virtual void OnDespawned() { }
    }
}