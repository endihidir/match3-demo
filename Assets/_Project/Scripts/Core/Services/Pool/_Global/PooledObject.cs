using System;
using UnityEngine;

namespace Core.Pool
{
    public class PooledObject : MonoBehaviour, IPooledObject
    {
        public int PoolKey { get; set; }
        public virtual bool IsActive => gameObject.activeInHierarchy;

        public virtual void Activate(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            gameObject.SetActive(true);
            onComplete?.Invoke();
        }

        public virtual void Deactivate(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
    
    public interface IPooledObject
    { 
        public int PoolKey { get; set; }
        public bool IsActive { get; }
        public void Activate(float duration = 0f, float delay = 0f, Action onComplete = null);
        public void Deactivate(float duration = 0f, float delay = 0f, Action onComplete = null);
    }
}