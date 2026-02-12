using System;
using Core.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
{
    public class BombFxView : BoosterFxView
    {
        [field: SerializeField] public ParticleFxModule ParticleFxModule { get; set; }
        
        public void Initialize(Vector3 pos, int radius)
        {
            transform.position = pos;
        }
        
        public override async UniTask Play(Action onComplete = null)
        {
            if (!ParticleFxModule) return;
            ParticleFxModule.Play();
            await UniTask.WaitUntil(() => !ParticleFxModule.IsAlive);
            onComplete?.Invoke();
        }

        public override void Stop()
        {
            if (!ParticleFxModule) return;
            ParticleFxModule.Stop();
        }
    }
}