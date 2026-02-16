using System;
using Cysharp.Threading.Tasks;

namespace Core.Views
{
    public interface ISceneTransitionView
    {
        UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null);
        UniTask DisableAsync(float duration = 0.2f, float delay = 0f, Action onComplete = null);
        void SetFillAmount(float value);
        void SetLabelText(string value);
        void SetPercentageText(string value);
    }
}