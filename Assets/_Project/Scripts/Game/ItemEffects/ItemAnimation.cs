using System;
using Core.Config;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Item
{
    public class ItemAnimation : MonoBehaviour
    {
        [field: SerializeField] private bool UseUnscaledTime { get; set; }= true;
        [field: SerializeField] private ShakeSettingsConfig ShakeSettingsConfig { get; set; }
        [field: SerializeField] private ShiftSettingsConfig ShiftSettingsConfig { get; set; }
        [field: SerializeField] private Transform ItemHolder { get; set; }
        
        private Tween _shakeTween, _moveTween;

        public void Shake()
        {
            var shakeSettings = ShakeSettingsConfig;

            _shakeTween?.Kill();

            _shakeTween = ItemHolder.DOShakePosition(shakeSettings.duration, shakeSettings.strength)
                                    .SetUpdate(UseUnscaledTime);
        }

        public Tween Shift(Vector3 worldPos)
        {
            var shiftSettings = ShiftSettingsConfig;

            _moveTween?.Kill();

            _moveTween = transform.DOMove(worldPos, shiftSettings.duration)
                                   .SetEase(shiftSettings.ease)
                                   .SetUpdate(UseUnscaledTime);

            return _moveTween;
        }

        public Tween PingPongMove(Vector3 targetPos, float duration, Ease ease, Action onComplete)
        {
            _moveTween?.Kill();
                
            var defaultPos = transform.position;

            _moveTween = DOTween.Sequence()
                                .Append(transform.DOMove(targetPos, duration).SetEase(ease))
                                .Append(transform.DOMove(defaultPos,   duration).SetEase(ease))
                                .SetUpdate(UseUnscaledTime)
                                .OnComplete(() => onComplete?.Invoke());
            
            return _moveTween;
        }

        public Tween Move(Vector3 worldPos, float duration, Ease ease = Ease.Linear, Action onComplete = null)
        {
            _moveTween?.Kill();
            
            _moveTween = transform.DOMove(worldPos, duration)
                                  .SetEase(ease)
                                  .OnComplete(()=> onComplete?.Invoke())
                                  .SetUpdate(UseUnscaledTime);

            return _moveTween;
        }

        public async UniTask ShiftAsync(Vector3 worldPos)
        {
            var tween = Shift(worldPos);
            await tween.AsyncWaitForCompletion();
        }

        public async UniTask MoveAsync(Vector3 worldPos, float duration, Ease ease, Action onComplete = null)
        {
            var tween = Move(worldPos, duration, ease, onComplete);
            await tween.AsyncWaitForCompletion();
        }

        public async UniTask PingPongMoveAsync(Vector3 targetPos, float duration, Ease ease = Ease.Linear, Action onComplete = null)
        {
            var tween = PingPongMove(targetPos, duration, ease, onComplete);
            await tween.AsyncWaitForCompletion();
        }
        
        public void Dispose()
        {
            _moveTween?.Kill();
            _shakeTween?.Kill();
        }

        private void OnDestroy() => Dispose();
    }
}