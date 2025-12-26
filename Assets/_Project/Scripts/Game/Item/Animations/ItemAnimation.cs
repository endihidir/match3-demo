using Core.Config;
using DG.Tweening;
using UnityEngine;

namespace Core.Item
{
    public class ItemAnimation : MonoBehaviour
    {
        [field: SerializeField] private bool UseUnscaledTime { get; set; }= true;
        [field: SerializeField] private ShakeSettingsConfig ShakeSettingsConfig { get; set; }
        [field: SerializeField] private Transform ItemHolder { get; set; }
        public bool IsShiftInProgress => _shiftTween.IsActive();
        
        private Tween _shakeTween, _moveTween, _shiftTween;

        public void Shake()
        {
            var shakeSettings = ShakeSettingsConfig;

            _shakeTween?.Kill(true);
            
            var duration = shakeSettings.duration / 3f;
            var rotAngle = shakeSettings.angle;

            _shakeTween = DOTween.Sequence()
                .Append(ItemHolder.transform.DORotate(Vector3.forward * rotAngle, duration))
                .Append(ItemHolder.transform.DORotate(Vector3.back * rotAngle, duration))
                .Append(ItemHolder.transform.DORotate(Vector3.zero, duration))
                .OnComplete(() => ItemHolder.transform.localRotation = Quaternion.identity)
                .SetUpdate(UseUnscaledTime);
        }

        public Tween Shift(Vector3 worldPos, float durationMultiplier = 1f, float delay = 0f)
        {
            _shiftTween?.Kill();
            
            _shiftTween = DOTween.Sequence()
                .Append(transform.DOMove(worldPos, 0.15f * durationMultiplier).SetEase(Ease.InOutQuad).SetDelay(delay))
                .SetUpdate(UseUnscaledTime);

            return _shiftTween;
        }

        public Tween PingPongMove(Vector3 targetPos, float duration = 0.15f, Ease ease = Ease.Linear)
        {
            _moveTween?.Kill();
                
            var defaultPos = transform.position;

            _moveTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, duration).SetEase(ease))
                                .Append(transform.DOMove(defaultPos, duration).SetEase(ease))
                                .SetUpdate(UseUnscaledTime);
            
            return _moveTween;
        }

        public Tween Move(Vector3 worldPos, float duration = 0.15f, Ease ease = Ease.Linear)
        {
            _moveTween?.Kill();
            
            _moveTween = transform.DOMove(worldPos, duration)
                                  .SetEase(ease)
                                  .SetUpdate(UseUnscaledTime);

            return _moveTween;
        }
        
        public void Dispose()
        {
            _shiftTween?.Kill();
            _moveTween?.Kill();
            _shakeTween?.Kill();
        }

        private void OnDestroy() => Dispose();
    }
}