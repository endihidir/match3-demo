using System;
using DG.Tweening;
using UnityEngine;

namespace Core.UI
{
    public abstract class BaseAnimatedFXView : BaseFxView
    {
        protected Tween _moveTween, _sizeTween, _bounceTween;
        
        public BaseAnimatedFXView MoveTo(Vector3 position, float duration, float delay = 0f, Ease ease = Ease.Linear)
        {
            _moveTween.Kill();

            _moveTween = transform.DOMove(position, duration)
                                  .SetEase(ease)
                                  .SetDelay(delay)
                                  .SetUpdate(UseUnscaledTime);
            
            return this;
        }
        
        public BaseAnimatedFXView PlayBounce(float duration = 0.22f, float up = 1.12f)
        {
            _bounceTween.Kill();
            
            _bounceTween = DOTween.Sequence()
                                  .Append(FxViewHolder.DOScale(1f * up, duration * 0.7f).SetEase(Ease.OutQuad))
                                  .Append(FxViewHolder.DOScale(1f,duration * 0.3f).SetEase(Ease.OutBack));

            return this;
        }

        public BaseAnimatedFXView OnMoveComplete(Action callback)
        {
            _moveTween.OnComplete(()=> callback?.Invoke());
            return this;
        }
        
        public BaseAnimatedFXView OnSizeComplete(Action callback)
        {
            _sizeTween.OnComplete(()=> callback?.Invoke());
            return this;
        }

        public BaseAnimatedFXView OnBounceComplete(Action callback)
        {
            _bounceTween.OnComplete(()=> callback?.Invoke());
            return this;
        }

        public abstract void SetSprite(Sprite sprite);
        public abstract void SetSize(Vector2 size);
        public abstract BaseAnimatedFXView SetSize(Vector2 size, float duration, float delay = 0f, Ease ease = Ease.Linear);

        protected override void OnDestroy()
        {
            _moveTween.Kill();
            _sizeTween.Kill();
            _bounceTween.Kill();
        }
    }
}