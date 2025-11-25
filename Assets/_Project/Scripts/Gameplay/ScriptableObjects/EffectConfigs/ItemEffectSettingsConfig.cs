using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemEffectSettings", menuName = "Match3/ItemConfigs/ItemEffectSettings", order = 0)]
    public class ItemEffectSettingsConfig : ScriptableObject
    {
        public ShakeSettingsConfig shakeSettings;
        public ShiftSettingsConfig shiftSettings;
    }
}