using DG.Tweening;
using UnityEngine;

namespace Core.Views
{
    public class BounceAnimationModule : MonoBehaviour
    {
        [field: SerializeField] public Transform BounceTransform { get; private set; }
        [field: SerializeField] private float Duration { get; set; } = 0.15f;
        [field: SerializeField] private float SizeUpMultiplier { get; set; } = 1.2f;
        
        private Tween _bounceTween;
        
        public void PlayBounce(bool useUnscaledTime = false)
        {
            _bounceTween.Kill(true);

            _bounceTween = DOTween.Sequence()
                                  .Append(BounceTransform.DOScale(SizeUpMultiplier, Duration * 0.7f).SetEase(Ease.OutQuad))
                                  .Append(BounceTransform.DOScale(1f, Duration * 0.3f).SetEase(Ease.OutBack))
                                  .SetUpdate(useUnscaledTime);
        }
        
        public void Dispose() => _bounceTween.Kill(true);
        private void OnDestroy() => Dispose();
    }
}