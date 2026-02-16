using Core.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
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
            ParticleFxModule.Play();
            await UniTask.WaitUntil(() => !ParticleFxModule.IsAlive);
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            Stop();
        }

        public override void Stop() => ParticleFxModule.Stop();
    }
}