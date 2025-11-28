using Core.Config;

namespace Core.Item
{
    public class BoosterEffect : BaseItemEffect<BoosterItemConfig>
    {
        private BoosterType _boosterType;
        private BoosterEffectConfig _effectConfig;

        protected override void OnInitialized()
        {
            _boosterType = (BoosterType)TypeId;
            _effectConfig = ItemConfig.GetEffectConfig(_boosterType);
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
            _boosterType = BoosterType.None;
        }
        
    }
}