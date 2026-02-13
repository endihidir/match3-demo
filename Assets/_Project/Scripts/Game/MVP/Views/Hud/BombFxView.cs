using Core.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
{
    public class BombFxView : BoosterFxView
    {
        [field: SerializeField] public ParticleFxModule ParticleFxModule { get; set; }

        public void ApplyData(float radius)
        {
            
        }
        
        public override async UniTask Play()
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