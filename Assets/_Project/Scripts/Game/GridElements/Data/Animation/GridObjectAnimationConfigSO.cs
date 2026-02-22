using UnityEngine;

namespace Game.Configs
{
    [CreateAssetMenu(fileName = "GridObjectAnimationConfig", menuName = "Game/Gameplay/Grid/GridObjectAnimationConfig")]
    public sealed class GridObjectAnimationConfigSO : ScriptableObject
    {
        [field: SerializeField] public bool UseUnscaledTime { get; private set; } = true;
        [field: SerializeField, Header("SHIFT SETTINGS")] public float ShiftDuration { get; private set; } = .18f;
        [field: SerializeField] public float ShiftDelay { get; private set; } = .03f;
        [field: SerializeField] public float ShiftDistanceMultiplier { get; private set; } = .05f;
        [field: SerializeField] public float StartShiftDelay { get; private set; } = .1f;
        [field: SerializeField, Header("SLIDE SETTINGS")] public float SlideDuration { get; private set; } = .03f;
        [field: SerializeField] public float SlideDelay { get; private set; } = .03f;
        [field: SerializeField] public float SlideDistanceMultiplier { get; private set; } = .05f;
        [field: SerializeField] public float StartSlideDelay { get; private set; } = .1f;
        [field: SerializeField, Header("MOVE SETTINGS")] public float MoveDuration { get; private set; } = .15f;
        [field: SerializeField] public float PingPongDuration { get; private set; } = .15f;
        [field: SerializeField, Header("SHAKE SETTINGS")] public float ShakeDuration { get; private set; } = .25f;
        [field: SerializeField] public float ShakeRotAngle { get; private set; } = 5f;
        [field: SerializeField, Header("SPRING SETTINGS")] public float SpringDuration { get; private set; } = .1f;
        [field: SerializeField] public Vector3 SpringScale { get; private set; } = new(1.08f, .92f, 1f);
        [field: SerializeField] public float SpringYMoveOffset { get; private set; } = .05f;
    }
}