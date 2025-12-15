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
        
        private Tween _shakeTween, _shiftTween;
        public bool IsInProgress => _shakeTween.IsActive() || _shiftTween.IsActive();

        public virtual void Shake()
        {
            var shakeSettings = ShakeSettingsConfig;

            _shakeTween?.Kill();

            _shakeTween = transform.DOShakePosition(shakeSettings.duration, shakeSettings.strength)
                                                 .SetUpdate(UseUnscaledTime);
        }

        public virtual Tween Shift(Vector3 worldPos)
        {
            var shiftSettings = ShiftSettingsConfig;

            _shiftTween?.Kill();

            _shiftTween = transform.DOMove(worldPos, shiftSettings.duration).SetUpdate(UseUnscaledTime);

            return _shiftTween;
        }

        public virtual async UniTask ShiftAsync(Vector3 worldPos)
        {
            var tween = Shift(worldPos);
            await tween.AsyncWaitForCompletion();
        }

        public virtual void ForceComplete()
        {
            _shakeTween?.Complete();
            _shiftTween?.Complete();
        }

        public virtual void Dispose()
        {
            _shakeTween?.Kill();
            _shiftTween?.Kill();
        }
    }
}