using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ShakeSettingsConfig", menuName = "Match3/ItemConfigs/Animations/Settings/ShakeSettings", order = 1)]
    public class ShakeSettingsConfig : ScriptableObject
    {
        public float duration = 0.2f;
        public float angle = 35f;
    }
}