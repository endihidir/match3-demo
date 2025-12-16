using DG.Tweening;
using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ShiftSettingsConfig", menuName = "Match3/ItemConfigs/Animations/Settings/ShiftSettings", order = 0)]
    public class ShiftSettingsConfig : ScriptableObject
    {
        public float duration = 0.25f;
        public float delay = 0f;
        public Ease ease = Ease.Linear;
    }
}