using Core.Config;
using Core.Extensions;
using Core.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Item
{
    public interface IItemAnimation
    {
        public bool IsInProgress { get; }
        void Initialize(ItemEffectSettingsConfig defaultSettings);
        void Shake();
        Tween Shift(IGridModel<IGridItemState> gridModel);
        UniTask ShiftAsync(IGridModel<IGridItemState> gridModel);
        void ForceComplete();
        void Dispose();
    }
    
    public class ItemAnimation : IItemAnimation
    {
        private readonly IItemObjectReader _owner;
        private ItemEffectSettingsConfig _defaultSettings;
        private Tween _shakeTween, _shiftTween;

        public bool IsInProgress => _shakeTween.IsActive() || _shiftTween.IsActive();
        public ItemAnimation(IItemObjectReader owner) => _owner = owner;
        public void Initialize(ItemEffectSettingsConfig defaultSettings) => _defaultSettings = defaultSettings;

        public virtual void Shake()
        {
            var shakeSettings = _defaultSettings.shakeSettings;

            _shakeTween?.Kill();

            _shakeTween = _owner.Transform.DOShakePosition(shakeSettings.duration, shakeSettings.strength);
        }

        public virtual Tween Shift(IGridModel<IGridItemState> gridModel)
        {
            var worldPos = gridModel.GridToWorld(_owner.GridPos);
            
            var shiftSettings = _defaultSettings.shiftSettings;

            _shiftTween?.Kill();

            _shiftTween = _owner.Transform.DOMove(worldPos, shiftSettings.duration);

            return _shiftTween;
        }

        public virtual async UniTask ShiftAsync(IGridModel<IGridItemState> gridModel)
        {
            var tween = Shift(gridModel);
            
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