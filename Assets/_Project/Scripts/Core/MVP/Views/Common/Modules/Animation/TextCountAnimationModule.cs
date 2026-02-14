using DG.Tweening;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

namespace Core.Modules
{
    public class TextCountAnimationModule : MonoBehaviour
    {
        [field: SerializeField, Required] public TextMeshProUGUI TextMeshProUGUI { get; private set; }

        private Tween _textTween;

        public void PlayTextCount(int endCount, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            if(!int.TryParse(TextMeshProUGUI.text, out var startCount)) return;
            
            Dispose();
            
            _textTween = DOVirtual.Int(startCount, endCount, duration, x => TextMeshProUGUI.SetText(x.ToString()))
                .SetDelay(delay)
                .SetEase(ease)
                .SetUpdate(useUnscaledTime);
        }
        
        public void Dispose() => _textTween.Kill(true);
        private void OnDestroy() => Dispose();
    }
}