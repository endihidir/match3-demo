using Core.Config;

namespace Core.Item
{
    public class ObstacleEffect : BaseItemEffect<ObstacleConfig>
    {
        private readonly ObstacleType _obstacleType;
        
        private readonly ObstacleEffectConfig _effectConfig;
        public ObstacleEffect(IItemObject itemObject, ObstacleType obstacleType, ObstacleConfig obstacleConfig) : base(itemObject, obstacleConfig)
        {
            _obstacleType = obstacleType;
            
            _effectConfig = ItemConfig.GetEffectConfig(_obstacleType);

        }

        protected override ShiftSettingsConfig GetShiftSettings() => _effectConfig.shiftSettingsConfig;
        protected override ShakeSettingsConfig GetShakeSettings() => _effectConfig.shakeSettingsConfig;
    }
}