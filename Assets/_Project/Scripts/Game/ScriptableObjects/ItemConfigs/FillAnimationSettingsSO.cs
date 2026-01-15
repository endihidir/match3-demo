using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "FillAnimationSettings", menuName = "Match3/ItemConfigs/FillAnimationSettings", order = 0)]
    public sealed class FillAnimationSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float ShiftDelayMultiplier { get; private set; } = 0.01f;
        [field: SerializeField] public float SlideDelayMultiplier { get; private set; } = 0.01f;
    }
}