using UnityEngine;

namespace Core.Pool
{
    public class PooledObject : MonoBehaviour, IPooledObject
    {
        public int PoolKey { get; set; }
        public virtual bool IsActive => gameObject.activeInHierarchy;

        public virtual void Activate() => gameObject.SetActive(true);
        public virtual void Deactivate() => gameObject.SetActive(false);
    }
}