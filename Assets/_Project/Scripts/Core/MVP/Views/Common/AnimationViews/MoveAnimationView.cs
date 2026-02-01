using DG.Tweening;
using UnityEngine;

namespace Core.Views
{
    public class MoveAnimationView : MonoBehaviour
    {
        private Tween _moveTween;
        
        public virtual Tween MoveTo(Vector3 position, float duration, float delay = 0f, Ease ease = Ease.Linear, bool useUnscaledTime = false)
        {
            _moveTween.Kill();

            _moveTween = transform.DOMove(position, duration)
                                    .SetEase(ease)
                                    .SetDelay(delay)
                                    .SetUpdate(useUnscaledTime);
            
            return _moveTween;
        }
        
        public void Dispose() => _moveTween.Kill(true);
        private void OnDestroy() => Dispose();
    }
}