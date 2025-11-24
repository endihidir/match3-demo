using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ShakeSettingsConfig", menuName = "Match3/ItemConfigs/Effects/Common/ShakeSettings", order = 1)]
    public class ShakeSettingsConfig : ScriptableObject
    {
        public float duration = 0.2f;
        public float strength = 0.5f;
        public int vibrato = 10;
    }
}