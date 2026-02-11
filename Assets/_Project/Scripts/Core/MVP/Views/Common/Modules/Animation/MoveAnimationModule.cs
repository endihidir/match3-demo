using DG.Tweening;
using UnityEngine;

namespace Core.Modules
{
    public class MoveAnimationModule : MonoBehaviour
    {
        [field: SerializeField] public Transform Transform { get; private set; }
        
        private Tween _moveTween;
        
        public virtual Tween MoveTo(Vector3 position, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _moveTween.Kill();
            
            var tr = Transform ?? transform;

            _moveTween = tr.DOMove(position, duration)
                                            .SetEase(ease)
                                            .SetDelay(delay)
                                            .SetUpdate(useUnscaledTime);
            
            return _moveTween;
        }
        
        public virtual Tween SetAnchoredPos(Vector3 position, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _moveTween.Kill();
            
            var tr = Transform ?? transform;
            
            if (tr is not RectTransform rectTransform) return _moveTween;

            _moveTween = rectTransform.DOAnchorPos(position, duration)
                                        .SetEase(ease)
                                        .SetDelay(delay)
                                        .SetUpdate(useUnscaledTime);
            
            return _moveTween;
        }
        
        public void Dispose() => _moveTween.Kill(true);
        private void OnDestroy() => Dispose();
    }
}