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
    
    public interface IPooledObject
    { 
        public int PoolKey { get; set; }
        public bool IsActive { get; }
        public void Activate();
        public void Deactivate();
    }
}