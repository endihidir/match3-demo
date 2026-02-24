using Core.Pool.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Views
{
    public abstract class BaseFxView : PooledObject
    {
        [field: SerializeField] protected bool UseUnscaledTime { get; private set; }
        [field: SerializeField] protected Transform FxViewHolder { get; private set; }
        
        public virtual UniTask PlayAsync() => UniTask.CompletedTask;
        public virtual void Stop(){}
    }
}