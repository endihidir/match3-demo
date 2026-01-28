using UnityEngine;

namespace Core.Config
{
    [CreateAssetMenu(fileName = "ItemAnimationSettings", menuName = "Match3/ItemConfigs/ItemAnimationSettings", order = 0)]
    public sealed class ItemAnimationSettings : ScriptableObject
    {
        [field: SerializeField] public bool UseUnscaledTime { get; private set; } = true;
        [field: SerializeField, Header("SHIFT SETTINGS")] public float BaseShiftDuration { get; private set; } = 0.15f;
        [field: SerializeField] public float BaseShiftDelay { get; private set; } = 0.1f;
        [field: SerializeField] public float ShiftDistanceMultiplier { get; private set; } = 0.05f;
        [field: SerializeField] public float ShiftEarlyStartSeconds { get; private set; } = 0.05f;
        [field: SerializeField] public float MinShiftTimelineDuration { get; private set; } = 0.03f;
        [field: SerializeField, Header("SLIDE SETTINGS")] public float BaseSlideDuration { get; private set; } = 0.15f;
        [field: SerializeField] public float BaseSlideDelay { get; private set; } = 0.1f;
        [field: SerializeField] public float SlideDistanceMultiplier { get; private set; } = 0.05f;
        [field: SerializeField] public float SlideEarlyStartSeconds { get; private set; } = 0.1f;
        [field: SerializeField] public float MinSlideTimelineDuration { get; private set; } = 0.03f;
        [field: SerializeField, Header("MOVE SETTINGS")] public float BaseMoveDuration { get; private set; } = 0.15f;
        [field: SerializeField] public float BasePingPongDuration { get; private set; } = 0.15f;
        [field: SerializeField, Header("SHAKE SETTINGS")] public float ShakeDuration { get; private set; } = 0.25f;
        [field: SerializeField] public float ShakeRotAngle { get; private set; } = 5f;
        [field: SerializeField, Header("SPRING SETTINGS")] public float SpringDuration { get; private set; } = .1f;
        [field: SerializeField] public Vector3 SpringScale { get; private set; } = new (1.08f, 0.92f, 1f);
        [field: SerializeField] public float SpringYMove { get; private set; } = .05f;
    }
}