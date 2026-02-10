using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
{
    public abstract class BlastFxView : BaseFxView
    {
        [field: SerializeField] public ParticleFxModule ParticleFxModule { get; private set; }

        public virtual async UniTask Play(Action onComplete = null)
        {
            ParticleFxModule.Play();
            await UniTask.WaitUntil(() => !ParticleFxModule.IsAlive);
            onComplete?.Invoke();
        }

        public virtual void Stop() => ParticleFxModule.Stop();
    }
}