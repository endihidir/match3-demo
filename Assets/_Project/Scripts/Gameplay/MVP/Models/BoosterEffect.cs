using Core.Config;

namespace Core.Item
{
    public class BoosterEffect : BaseItemEffect<BoosterConfig>
    {
        private readonly BoosterType _boosterType;
        
        private readonly BoosterEffectConfig _effectConfig;
        public BoosterEffect(IItemObject itemObject, BoosterType boosterType, BoosterConfig boosterConfig) : base(itemObject, boosterConfig)
        {
            _boosterType = boosterType;
            
            _effectConfig = ItemConfig.GetEffectConfig(_boosterType);
        }
        
        protected override ShiftSettingsConfig GetShiftSettings() => _effectConfig.shiftSettingsConfig;
        protected override ShakeSettingsConfig GetShakeSettings() => _effectConfig.shakeSettingsConfig;
    }
}