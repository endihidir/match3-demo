using UnityEngine;

namespace Core.Config
{
    public abstract class BaseEffectConfig : ScriptableObject
    {
        public bool useUnscaledTime;
        public ShiftSettingsConfig shiftSettingsConfig;
        public ShakeSettingsConfig shakeSettingsConfig;
    }
}