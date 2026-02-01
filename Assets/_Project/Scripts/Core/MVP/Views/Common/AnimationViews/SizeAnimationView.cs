using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Views
{
    public class SizeAnimationView : MonoBehaviour
    {
        [field: SerializeField, HideIf(nameof(HasRectOrRenderer))] private Transform Transform { get; set; }
        [field: SerializeField, HideIf(nameof(HasTransformOrRenderer))] private RectTransform RectTransform { get; set; }
        [field: SerializeField, HideIf(nameof(HasTransformOrRect))] private SpriteRenderer SpriteRenderer { get; set; }
        
        private Tween _sizeTween;
        
        private bool HasTransformOrRenderer => Transform || SpriteRenderer;
        private bool HasRectOrRenderer => RectTransform || SpriteRenderer;
        private bool HasTransformOrRect => Transform || RectTransform;
        
        
        public Tween SetRectSize(Vector2 size, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _sizeTween?.Kill();
            
            _sizeTween = RectTransform?.DOSizeDelta(size, duration)
                                        .SetEase(ease)
                                        .SetDelay(delay)
                                        .SetUpdate(useUnscaledTime);
            
            return _sizeTween;
        }
        
        public Tween SetScale(Vector3 size, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _sizeTween?.Kill();
            
            _sizeTween = Transform?.DOScale(size, duration)
                                    .SetEase(ease)
                                    .SetDelay(delay)
                                    .SetUpdate(useUnscaledTime);
            
            return _sizeTween;
        }
        
        
        public Tween SetRendererSize(Vector2 size, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _sizeTween?.Kill();

            var defaultSize = SpriteRenderer.size;
            
            _sizeTween = DOVirtual.Vector2(defaultSize, size, duration, s=> SpriteRenderer.size = s)
                .SetEase(ease)
                .SetDelay(delay)
                .SetUpdate(useUnscaledTime);
            
            return _sizeTween;
        }
        
        public void Dispose() => _sizeTween.Kill(true);
        private void OnDestroy() => Dispose();
    }
}