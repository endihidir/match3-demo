using System;
using Core.Modules;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.UI
{
    public class RocketFxView : BoosterFxView
    {
        [field: SerializeField] private AnimatorModule AnimatorModule { get; set; }
        
        public override async UniTask Play(Action onComplete = null)
        {
            if(!AnimatorModule) return;
            AnimatorModule.Play();
            await UniTask.Yield();
            await UniTask.WaitUntil(()=> !AnimatorModule.IsAlive);
            onComplete?.Invoke();
        }

        public override void Stop()
        {
            if(!AnimatorModule) return;
            AnimatorModule.Stop();
        }
    }
}