using System;
using UnityEngine;

namespace Core.Pool
{
    public class PooledObject : MonoBehaviour, IPooledObject
    {
        public int PoolKey { get; set; }
        public bool IsActive => gameObject.activeInHierarchy;

        public virtual void Show(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            gameObject.SetActive(true);
            onComplete?.Invoke();
        }

        public virtual void Hide(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
    
    public interface IPooledObject
    { 
        public int PoolKey { get; set; }
        public bool IsActive { get; }
        public void Show(float duration = 0f, float delay = 0f, Action onComplete = null);
        public void Hide(float duration = 0f, float delay = 0f, Action onComplete = null);
    }
}