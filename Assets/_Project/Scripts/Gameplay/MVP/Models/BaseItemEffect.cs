using Core.Config;
using Core.Systems;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Core.Item
{
    public interface IBaseItemEffect
    {
        void Shake();
        Tween Shift(IGridModel gridModel);
        UniTask ShiftAsync(IGridModel gridModel);
    }
    
    public abstract class BaseItemEffect<T> : IBaseItemEffect where T : BaseItemConfig
    {
        protected readonly IItemObject ItemObject;
        protected readonly T ItemConfig;

        protected BaseItemEffect(IItemObject item, T config)
        {
            ItemObject = item;
            ItemConfig = config;
        }

        public virtual void Shake()
        {
            
        }

        public virtual Tween Shift(IGridModel gridModel)
        {
            var worldPos = gridModel.GridToWorld(ItemObject.GridPos);
            
        
            return default;
        }

        public virtual async UniTask ShiftAsync(IGridModel gridModel)
        {
            var worldPos = gridModel.GridToWorld(ItemObject.GridPos);
            
        }

        private ShiftSettingsConfig SelectShiftSettings() => GetShiftSettings() ?? ItemConfig.defaultShiftSettings;
        
        private ShakeSettingsConfig SelectShakeSettings() => GetShakeSettings() ?? ItemConfig.defaultShakeSettings;
        
        protected abstract ShiftSettingsConfig GetShiftSettings();
        protected abstract ShakeSettingsConfig GetShakeSettings();
    }
}