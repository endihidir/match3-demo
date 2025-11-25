using Core.Config;

namespace Core.Item
{
    public class RegularEffect : BaseItemEffect<ItemConfig>
    {
        private ItemType _itemType;
        private RegularEffectConfig _effectConfig;

        protected override void OnInitialized()
        {
            _itemType = (ItemType)TypeId;
            _effectConfig = ItemConfig.GetEffectConfig(_itemType);
        }
        
        protected override ShiftSettingsConfig GetShiftSettings()
        {
            var canGet = _effectConfig.TryGetShiftSettings(out var settings);
            return canGet ? settings : null;
        }

        protected override ShakeSettingsConfig GetShakeSettings()
        {
            var canGet = _effectConfig.TryGetShakeSettings(out var settings);
            return canGet ? settings : null;
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _effectConfig = null;
            _itemType = ItemType.None;
        }
    }
}