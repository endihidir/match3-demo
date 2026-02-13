using Core.Pool;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
{
    public abstract class BaseFxView : PooledObject
    {
        [field: SerializeField] protected bool UseUnscaledTime { get; private set; }
        [field: SerializeField] protected Transform FxViewHolder { get; private set; }
        
        public virtual UniTask Play() => UniTask.CompletedTask;
        public virtual void Stop(){}
    }
}