using Core.Config;
using Core.Extensions;
using Core.Item.Factories;
using Core.Models;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Item
{
    public interface IBaseItemEffect
    {
        public bool IsInProgress { get; }
        IBaseItemEffect Initialize(EffectData effectData);
        void Shake();
        Tween Shift(IGridModel<IGridItemBehaviour> gridModel);
        UniTask ShiftAsync(IGridModel<IGridItemBehaviour> gridModel);
        void ForceComplete();
        void Dispose();
    }
    
    public abstract class BaseItemEffect<T> : IBaseItemEffect where T : BaseItemConfig
    {
        protected IItemObject Owner;
        protected T ItemConfig;
        protected int TypeId;
        
        private ItemEffectSettingsConfig _defaultSettings;
        private Tween _shakeTween, _shiftTween;

        public bool IsInProgress => _shakeTween.IsActive() || _shiftTween.IsActive();
        
        public IBaseItemEffect Initialize(EffectData effectData)
        {
            Owner = effectData.owner;
            ItemConfig = (T)effectData.itemConfig;
            TypeId = effectData.typeId;
            _defaultSettings = effectData.defaultSettings;
            OnInitialized();
            return this;
        }

        protected abstract void OnInitialized();

        public virtual void Shake()
        {
            var shakeSettings = SelectedShakeSettings();

            _shakeTween?.Kill();

            _shakeTween = Owner.Transform.DOShakePosition(shakeSettings.duration, shakeSettings.strength);
        }

        public virtual Tween Shift(IGridModel<IGridItemBehaviour> gridModel)
        {
            var worldPos = gridModel.GridToWorld(Owner.GridPos);
            
            var shiftSettings = SelectedShiftSettings();

            _shiftTween?.Kill();

            _shiftTween = Owner.Transform.DOMove(worldPos, shiftSettings.duration);

            return _shiftTween;
        }

        public virtual async UniTask ShiftAsync(IGridModel<IGridItemBehaviour> gridModel)
        {
            var tween = Shift(gridModel);
            
            await tween.AsyncWaitForCompletion();
        }

        public virtual void ForceComplete()
        {
            _shakeTween?.Complete();
            _shiftTween?.Complete();
        }

        private ShiftSettingsConfig SelectedShiftSettings() => GetShiftSettings() ?? _defaultSettings.shiftSettings;
        private ShakeSettingsConfig SelectedShakeSettings() => GetShakeSettings() ?? _defaultSettings.shakeSettings;

        protected abstract ShiftSettingsConfig GetShiftSettings();
        protected abstract ShakeSettingsConfig GetShakeSettings();

        public virtual void Dispose()
        {
            _shakeTween?.Kill();
            _shiftTween?.Kill();

            Owner = null;
            ItemConfig = null;
            TypeId = 0;
        }
    }
}