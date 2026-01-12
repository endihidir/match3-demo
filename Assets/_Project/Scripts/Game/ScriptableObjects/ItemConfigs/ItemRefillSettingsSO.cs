using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "RefillSettings", menuName = "Match3/ItemConfigs/RefillSettings", order = 0)]
    public sealed class RefillSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float ShiftDelayMultiplier { get; private set; } = 0.01f;
        [field: SerializeField] public float SlideDelayMultiplier { get; private set; } = 0.01f;
    }
}