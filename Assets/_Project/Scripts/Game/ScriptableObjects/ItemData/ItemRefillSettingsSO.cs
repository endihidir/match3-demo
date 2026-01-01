using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "RefillSettings", menuName = "Match3/ItemConfigs/RefillSettings", order = 0)]
    public sealed class RefillSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float ShiftDurationMultiplier { get; private set; } = 0.1f;
        [field: SerializeField] public float ShiftDelayMultiplier { get; private set; } = 0.01f;
        [field: SerializeField] public SpawnSettings SpawnSettings { get; private set; }
    }
}