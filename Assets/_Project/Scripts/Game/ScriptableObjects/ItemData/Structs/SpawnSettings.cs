using System;
using UnityEngine;

namespace Core.Config
{
    /// <summary>
    /// External configuration for spawn behavior.
    /// All tuning is done via this struct.
    /// </summary>
    [Serializable]
    public struct SpawnSettings
    {
        // 0 = never allow immediate match (if possible)
        // 1 = fully allow immediate match
        [field: SerializeField, Range(0f, 1f)] public float AllowImmediateMatch { get; private set; }

        // Penalty for near-match (2-in-a-row / adjacency) setups
        [field: SerializeField, Range(0f, 1f)] public float NearMatchPenalty{ get; private set; }
    }
}