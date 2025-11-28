using NaughtyAttributes;
using UnityEngine;

namespace Core.Config
{
    public abstract class BaseEffectConfig : ScriptableObject
    {
        public bool useUnscaledTime;
        
        [Header("Optional overrides")]
        
        public bool overrideShake;
        
        [field: SerializeField, ShowIf(nameof(overrideShake))]
        private ShakeSettingsConfig ShakeSettings { get; set; }

        public bool overrideShift;
        [field: SerializeField, ShowIf(nameof(overrideShift))]
        private ShiftSettingsConfig ShiftSettings { get; set; }

        public bool TryGetShakeSettings(out ShakeSettingsConfig shakeSettingsConfig)
        {
            if (overrideShake)
            {
                shakeSettingsConfig = ShakeSettings;
                return true;
            }

            shakeSettingsConfig = null;
            return false;
        }
        
        public bool TryGetShiftSettings(out ShiftSettingsConfig shiftSettingsConfig)
        {
            if (overrideShake)
            {
                shiftSettingsConfig = ShiftSettings;
                return true;
            }

            shiftSettingsConfig = null;
            return false;
        }
        
    }
}