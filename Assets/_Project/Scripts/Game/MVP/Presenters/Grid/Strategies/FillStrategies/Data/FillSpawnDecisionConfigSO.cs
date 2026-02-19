using UnityEngine;

namespace Game.Configs
{
    /// <summary>
    /// External configuration for spawn behavior.
    /// All tuning is done via this struct.
    /// </summary>
    [CreateAssetMenu(fileName = "FillSpawnDecisionConfig", menuName = "Game/Gameplay/Grid/FillSpawnDecisionConfig", order = 0)]
    public class FillSpawnDecisionConfigSO : ScriptableObject
    {
        // 0 = never allow immediate match (if possible)
        // 100 = fully allow immediate match
        [field: SerializeField, Range(0f, 100f)]
        public float ImmediateMatchChance { get; private set; } = 0f;

        // Penalty for near-match (2-in-a-row / adjacency) setups
        [field: SerializeField, Range(0f, 100f)]
        public float NearMatchAvoidance { get; private set; } = 100f;
    }
}