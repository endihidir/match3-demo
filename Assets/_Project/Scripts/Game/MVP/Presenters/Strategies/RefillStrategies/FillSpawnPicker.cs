using System;
using Core.Config;
using Core.Configs;
using Core.Item;
using Core.Models;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    /// <summary>
    /// Smart, controlled spawn decision logic.
    /// 
    /// Goals:
    /// - Prevent immediate (on-spawn) matches when desired
    /// - Allow controlled match probability for difficulty tuning
    /// - Avoid infinite refill / cascade loops
    /// - Be deterministic when seeded; otherwise use Unity RNG
    /// </summary>
    
    public class FillSpawnPicker : IFillSpawnPicker
    {
        private readonly ItemType[] _allSpawnableTypes = GridMatchPatternUtil.BuildAllSpawnableTypes();
        
        private readonly SpawnSettingsSO _spawnSettings;
        
        private System.Random _rng;
        public float SafetyBoost { get; private set; }
        
        public FillSpawnPicker(GameplayConfigContainer gameplayConfigContainer)
        {
            _spawnSettings = gameplayConfigContainer.SpawnSettings;
            _rng = null;
        }
        
        public void SetSeed(int seed) => _rng = new System.Random(seed);
        
        public void ClearSeed() => _rng = null;
        private int NextInt(int minInclusive, int maxExclusive) => _rng?.Next(minInclusive, maxExclusive) ?? 
                                                                   UnityEngine.Random.Range(minInclusive, maxExclusive);
        private float Next01() => _rng != null ? (float)_rng.NextDouble() : UnityEngine.Random.value;

        /// <summary>
        /// Main entry point.
        /// Decides which item type should be spawned at the given cell.
        /// External safety bias (long refill chains etc.)
        /// </summary>
        public ItemType Decide(IGridModel model, Vector2Int targetCoord)
        {
            var types = _allSpawnableTypes;
            if (types.Length == 0)
                return ItemType.None;

            Span<ItemType> safe = stackalloc ItemType[64];
            Span<ItemType> match = stackalloc ItemType[64];
            var safeCount = 0;
            var matchCount = 0;

            for (int i = 0; i < types.Length; i++)
            {
                var type = types[i];

                // Avoid vertical stacks regardless of fill order (above OR below)
                if (GridMatchPatternUtil.IsSame(model, targetCoord.x, targetCoord.y + 1, type) ||
                    GridMatchPatternUtil.IsSame(model, targetCoord.x, targetCoord.y - 1, type))
                    continue;

                if (GridMatchPatternUtil.CreatesImmediateMatch(model, targetCoord, type))
                    match[matchCount++] = type;
                else
                    safe[safeCount++] = type;
            }

            // If everything got filtered out (edge case), relax and pick from all types.
            if (safeCount == 0 && matchCount == 0)
                return types[NextInt(0, types.Length)];

            var immediateChance01 = Mathf.Clamp01((_spawnSettings.ImmediateMatchChance - SafetyBoost) / 100f);

            var pickMatch = false;

            if (matchCount > 0 && safeCount > 0)
                pickMatch = Next01() < immediateChance01;
            else if (matchCount > 0)
                pickMatch = true;

            return pickMatch
                ? GridMatchPatternUtil.PickBest(model, targetCoord, match, matchCount, _spawnSettings, Next01())
                : GridMatchPatternUtil.PickBest(model, targetCoord, safe, safeCount, _spawnSettings, Next01());
        }
    }
}