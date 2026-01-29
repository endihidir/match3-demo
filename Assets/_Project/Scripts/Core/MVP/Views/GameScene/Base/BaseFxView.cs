using Core.Pool;
using UnityEngine;

namespace Core.UI
{
    public abstract class BaseFxView : PooledObject
    {
        [field: SerializeField] protected bool UseUnscaledTime { get; private set; }
        [field: SerializeField] protected Transform FxViewHolder { get; private set; }

        protected abstract void OnDestroy();
    }
}