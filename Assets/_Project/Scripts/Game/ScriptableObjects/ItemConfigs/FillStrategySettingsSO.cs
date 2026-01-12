using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "FillStrategySettings", menuName = "Match3/ItemConfigs/FillStrategySettings", order = 0)]
    public sealed class FillStrategySettingsSO : ScriptableObject
    {
        [field: SerializeField] public float ShiftDelayMultiplier { get; private set; } = 0.01f;
        [field: SerializeField] public float SlideDelayMultiplier { get; private set; } = 0.01f;
    }
}