using DG.Tweening;
using UnityEngine;

namespace Core.Modules
{
    public class SizeAnimationModule : MonoBehaviour
    {
        [field: SerializeField] public Transform Transform { get; private set; }
        
        private Tween _sizeTween;
        
        public Tween SetRectSize(Vector2 size, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _sizeTween?.Kill();
            
            var tr = Transform ?? transform;

            if (tr is not RectTransform rectTransform) return _sizeTween;
            
            _sizeTween = rectTransform?.DOSizeDelta(size, duration)
                                        .SetEase(ease)
                                        .SetDelay(delay)
                                        .SetUpdate(useUnscaledTime);
            
            return _sizeTween;
        }
        
        public Tween SetScale(Vector3 size, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _sizeTween?.Kill();
            
            var tr = Transform ?? transform;
            
            _sizeTween = tr?.DOScale(size, duration)
                                    .SetEase(ease)
                                    .SetDelay(delay)
                                    .SetUpdate(useUnscaledTime);
            
            return _sizeTween;
        }
        
        public void Dispose() => _sizeTween.Kill(true);
        private void OnDestroy() => Dispose();
    }
}