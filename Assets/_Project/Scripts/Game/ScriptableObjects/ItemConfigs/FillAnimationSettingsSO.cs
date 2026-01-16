using UnityEngine;

namespace Core.Config
{
    //[CreateAssetMenu(fileName = "FillAnimationSettings", menuName = "Match3/ItemConfigs/FillAnimationSettings", order = 0)]
    public sealed class FillAnimationSettingsSO : ScriptableObject
    {
        [field: SerializeField] public float ShiftDelay { get; private set; } = 0.03f;
        [field: SerializeField] public float SlideDelay { get; private set; } = 0.03f;
    }
}