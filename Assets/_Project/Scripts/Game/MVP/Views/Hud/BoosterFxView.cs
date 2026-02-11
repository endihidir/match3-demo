using System;
using Cysharp.Threading.Tasks;

namespace Core.UI
{
    public abstract class BoosterFxView : BaseFxView
    {
        public abstract UniTask Play(Action onComplete = null);
        public abstract void Stop();
    }
}