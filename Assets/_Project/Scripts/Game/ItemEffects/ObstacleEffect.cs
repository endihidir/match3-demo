using Core.Config;

namespace Core.Item
{
    public class ObstacleEffect : BaseItemEffect<ObstacleItemConfig>
    {
        private ObstacleType _obstacleType;
        private ObstacleEffectConfig _effectConfig;

        protected override void OnInitialized()
        {
            _obstacleType = (ObstacleType)TypeId;
            _effectConfig = ItemConfig.GetEffectConfig(_obstacleType);
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
            _obstacleType = ObstacleType.None;
        }
    }
}