using Core.Config;
using Core.Utils;
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
        
        public Tween ShiftPath(Vector3[] worldPoints, float durationMultiplier, float delay = 0f)
        {
            _shiftTween?.Kill();

            var sequence = DOTween.Sequence();

            var current = transform.position;

            foreach (var next in worldPoints)
            {
                var segDist = Mathf.Abs(current.y - next.y);
                var distCells = segDist / 2f;
                var durMul = 1f + distCells * durationMultiplier;
                var duration = 0.15f * durMul;

                sequence.Append(transform.DOMove(next, duration).SetEase(Ease.InOutQuad).SetDelay(delay))
                        .SetUpdate(UseUnscaledTime);;

                current = next;
            }

            _shiftTween = sequence;
            
            return _shiftTween;
        }

        public Tween PingPongMove(Vector3 targetPos)
        {
            _moveTween?.Kill();
                
            var defaultPos = transform.position;

            _moveTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, 0.15f).SetEase(Ease.Linear))
                                .Append(transform.DOMove(defaultPos, 0.15f).SetEase(Ease.Linear))
                                .SetUpdate(UseUnscaledTime);
            
            return _moveTween;
        }

        public Tween Move(Vector3 worldPos)
        {
            _moveTween?.Kill();
            
            _moveTween = transform.DOMove(worldPos, 0.15f)
                                  .SetEase(Ease.Linear)
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