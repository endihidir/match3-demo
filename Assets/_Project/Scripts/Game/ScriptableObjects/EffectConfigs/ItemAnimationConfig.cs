using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemAnimationConfig", menuName = "Match3/ItemConfigs/Animations/ItemAnimationConfig", order = 0)]
    public class ItemAnimationConfig : ScriptableObject
    {
        [field: SerializeField] public bool UseUnscaledTime{ get; private set; }
        [field: SerializeField] public ShakeSettingsConfig ShakeSettings { get; private set; }
        [field: SerializeField] public ShiftSettingsConfig ShiftSettings { get; private set; }
    }
}