using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public class FadeAnimationView : MonoBehaviour
    {
        [field: SerializeField, HideIf(nameof(HasGraphic))] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField, HideIf(nameof(HasCanvasGroup))] private Graphic Graphic { get; set; }
        
        private Tween _fadeTween;
        
        private bool HasCanvasGroup => CanvasGroup;
        private bool HasGraphic => Graphic;

        public async UniTask FadeInAsync(float duration = 0f, float delay = 0f, Action onComplete = null, bool useUnscaledTime = false)
        {
            _fadeTween.Kill(true);
            
            SetInteractable(true);
            
            _fadeTween = Graphic ? Graphic.DOFade(1f, duration) : CanvasGroup.DOFade(1f, duration);
            
            await _fadeTween.SetEase(Ease.Linear).SetDelay(delay).SetUpdate(useUnscaledTime);
            
            onComplete?.Invoke();
        }

        public async UniTask FadeOutAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null, bool useUnscaledTime = false)
        {
            _fadeTween.Kill(true);
            
            _fadeTween = Graphic ? Graphic.DOFade(0f, duration) : CanvasGroup.DOFade(0f, duration);

            await _fadeTween.SetEase(Ease.Linear).SetDelay(delay).SetUpdate(useUnscaledTime);
            
            SetInteractable(false);
            
            onComplete?.Invoke();
        }
        
        private void SetInteractable(bool value)
        {
            if (CanvasGroup)
            {
                CanvasGroup.interactable = value;
                CanvasGroup.blocksRaycasts = value;
            }

            if (Graphic)
            {
                Graphic.raycastTarget = value;
            }
        }

        public void Dispose() => _fadeTween.Kill();

        private void OnDestroy() => Dispose();
    }
}