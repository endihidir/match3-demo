using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public interface IFadeAnimationView : IDisposable
    {
        IFadeAnimationView Initialize(Graphic graphic);
        IFadeAnimationView Initialize(CanvasGroup canvasGroup);
        UniTask FadeInAsync(float duration = 0f, float delay = 0f, Action onComplete = null);
        UniTask FadeOutAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null);
    }
    
    public class FadeAnimationView : IFadeAnimationView
    {
        private CanvasGroup _canvasGroup;
        private Graphic _graphic;
        private Tween _sliderTween;

        public IFadeAnimationView Initialize(Graphic graphic)
        {
            _graphic = graphic;
            return this;
        }
        
        public IFadeAnimationView Initialize(CanvasGroup canvasGroup)
        {
            _canvasGroup = canvasGroup;
            return this;
        }

        public async UniTask FadeInAsync(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            _sliderTween.Kill(true);
            
            SetInteractable(true);
            
            _sliderTween = _graphic ? _graphic.DOFade(1f, duration) : _canvasGroup.DOFade(1f, duration);
            
            _sliderTween.SetEase(Ease.Linear).SetDelay(delay);
            
            await _sliderTween.AsyncWaitForCompletion();
            
            onComplete?.Invoke();
        }

        public async UniTask FadeOutAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null)
        {
            _sliderTween.Kill(true);
            
            _sliderTween = _graphic ? _graphic.DOFade(0f, duration) : _canvasGroup.DOFade(0f, duration);

            _sliderTween.SetEase(Ease.Linear).SetDelay(delay);
            
            await _sliderTween.AsyncWaitForCompletion();
            
            SetInteractable(false);
            
            onComplete?.Invoke();
        }
        
        private void SetInteractable(bool value)
        {
            if (_canvasGroup)
            {
                _canvasGroup.interactable = value;
                _canvasGroup.blocksRaycasts = value;
            }

            if (_graphic)
            {
                _graphic.raycastTarget = value;
            }
        }

        public void Dispose() => _sliderTween.Kill();
    }
}