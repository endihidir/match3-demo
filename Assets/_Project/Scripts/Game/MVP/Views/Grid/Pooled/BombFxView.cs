using Core.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Views
{
    public class BombFxView : BoosterFxView
    {
        [field: SerializeField] public ParticleFxModule ParticleFxModule { get; set; }
        
        public void ApplyData(float radius, float cellSize, float animSpeed)
        {
            var scale = (radius + 1) * cellSize;
            ParticleFxModule.SetLocalScale(scale * Vector3.one);
            ParticleFxModule.SetSimulationSpeed(animSpeed);
        }
        
        public override async UniTask PlayAsync()
        {
            ParticleFxModule.Restart();
            await UniTask.Yield();
            await UniTask.WaitUntil(() => !ParticleFxModule.HasAliveParticles);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            Stop();
        }

        public override void Stop() => ParticleFxModule.Stop();
    }
}