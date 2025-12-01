using System;
using Core.Extensions;
using Core.MVPContext.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public interface IFadeAnimationView : IView, IDisposable
    {
        UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null);
        UniTask DisableAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null);
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

        public async UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            _sliderTween.Kill(true);
            
            _sliderTween = _graphic ? _graphic.DOFadeSafe(1f, duration) : _canvasGroup.DOFadeSafe(1f, duration);
            
            _sliderTween.SetEase(Ease.Linear).SetDelay(delay);

            await _sliderTween.AsyncWaitForCompletion();
            
            onComplete?.Invoke();
        }

        public async UniTask DisableAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null)
        {
            _sliderTween.Kill(true);
            
            _sliderTween = _graphic ? _graphic.DOFadeSafe(0f, duration) : _canvasGroup.DOFadeSafe(0f, duration);
            
            _sliderTween.SetEase(Ease.Linear).SetDelay(delay);

            await _sliderTween.AsyncWaitForCompletion();
            
            onComplete?.Invoke();
        }

        public void Dispose()
        {
            _sliderTween.Kill();
        }
    }
}