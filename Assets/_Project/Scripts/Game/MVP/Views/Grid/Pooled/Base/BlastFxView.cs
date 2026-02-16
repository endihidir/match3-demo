using System;
using Core.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
{
    public abstract class BlastFxView : BaseFxView
    {
        [field: SerializeField] public ParticleFxModule ParticleFxModule { get; private set; }
        
        public override async UniTask PlayAsync()
        {
            if (!ParticleFxModule) return;
            ParticleFxModule.Play();
            await UniTask.WaitUntil(() => !ParticleFxModule.IsAlive);
        }

        public override void Stop()
        {
            if (!ParticleFxModule) return;
            ParticleFxModule.Stop();
        }
    }
}