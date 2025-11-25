using Core.Config;
using Core.Systems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Item
{
    public interface IBaseItemEffect
    {
        public bool IsInProgress { get; }
        void Initialize(IItemObject itemObject, BaseItemConfig config, int typeId, ItemEffectSettingsConfig defaultSettings);
        void Shake();
        Tween Shift(IGridModel gridModel);
        UniTask ShiftAsync(IGridModel gridModel);
        void ForceComplete();
        void Dispose();
    }
    
    public abstract class BaseItemEffect<T> : IBaseItemEffect where T : BaseItemConfig
    {
        protected IItemObject ItemObject;
        protected T ItemConfig;
        protected int TypeId;

        private Tween _shakeTween;
        private Tween _shiftTween;
        private ItemEffectSettingsConfig _defaultSettings;

        public bool IsInProgress => _shakeTween != null || _shiftTween != null;
        
        public void Initialize(IItemObject itemObject, BaseItemConfig config, int typeId, ItemEffectSettingsConfig defaultSettings)
        {
            ItemObject = itemObject;
            ItemConfig = (T)config;
            TypeId = typeId;
            _defaultSettings = defaultSettings;
            OnInitialized();
        }

        protected abstract void OnInitialized();

        public virtual void Shake()
        {
            var shakeSettings = SelectedShakeSettings();

            _shakeTween?.Kill();

            _shakeTween = ItemObject.Transform.DOShakePosition(shakeSettings.duration, shakeSettings.strength);
        }

        public virtual Tween Shift(IGridModel gridModel)
        {
            var worldPos = gridModel.GridToWorld(ItemObject.GridPos);
            
            var shiftSettings = SelectedShiftSettings();

            _shiftTween?.Kill();

            _shiftTween = ItemObject.Transform.DOMove(worldPos, shiftSettings.duration);

            return _shiftTween;
        }

        public virtual async UniTask ShiftAsync(IGridModel gridModel)
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

            ItemObject = null;
            ItemConfig = null;
            TypeId = 0;
        }
    }
}