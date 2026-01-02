using System;
using Core.Config;
using Core.Item;
using Core.Models;
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
    /// - Stay deterministic and stateless (pure function)
    /// </summary>
    public static class SmartSpawnDecider
    {
        private static readonly ItemType[] AllSpawnableTypes = BuildAllSpawnableTypes();
        /// <summary>
        /// Main entry point.
        /// Decides which item type should be spawned at the given cell.
        /// External safety bias (long refill chains etc.)
        /// </summary>
        public static ItemType Decide(IGridModel model, Vector2Int coord, SpawnSettings settings, float safetyBoost = 0f)
        {
            var types = AllSpawnableTypes;
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
                if (IsSame(model, coord.x, coord.y + 1, type) || IsSame(model, coord.x, coord.y - 1, type))
                    continue;

                if (CreatesImmediateMatch(model, coord, type))
                    match[matchCount++] = type;
                else
                    safe[safeCount++] = type;
            }

            // If everything got filtered out (edge case), relax and pick from all types.
            if (safeCount == 0 && matchCount == 0)
                return types[UnityEngine.Random.Range(0, types.Length)];

            var immediateChance01 = Mathf.Clamp01((settings.ImmediateMatchChance - safetyBoost) / 100f);
            var pickMatch = false;

            if (matchCount > 0 && safeCount > 0)
                pickMatch = UnityEngine.Random.value < immediateChance01;
            else if (matchCount > 0)
                pickMatch = true;

            return pickMatch
                ? PickBest(model, coord, match, matchCount, settings)
                : PickBest(model, coord, safe, safeCount, settings);
        }

        private static ItemType PickBest(IGridModel model, Vector2Int cell, Span<ItemType> types, int count, SpawnSettings settings)
        {
            var best = types[0];
            var bestScore = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var type = types[i];

                var score = 0f;

                // Penalize near-match setups (adjacency / 2-in-a-row potential)
                if (settings.NearMatchAvoidance > 0f)
                {
                    var nearAvoid01 = settings.NearMatchAvoidance / 100f;
                    score += nearAvoid01 * NearMatchScore(model, cell, type);
                }

                // Small noise to avoid always picking the same type
                score += UnityEngine.Random.value * 0.01f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = type;
                }
            }

            return best;
        }

        // Immediate match check (left2 / down2)
        private static bool CreatesImmediateMatch(IGridModel model, Vector2Int cell, ItemType type)
        {
            // Horizontal: XX_ , _XX , X_X
            if (IsSame(model, cell.x - 1, cell.y, type) && IsSame(model, cell.x - 2, cell.y, type))
                return true;

            if (IsSame(model, cell.x + 1, cell.y, type) && IsSame(model, cell.x + 2, cell.y, type))
                return true;

            if (IsSame(model, cell.x - 1, cell.y, type) && IsSame(model, cell.x + 1, cell.y, type))
                return true;

            // Vertical: XX_ , _XX , X_X
            if (IsSame(model, cell.x, cell.y - 1, type) && IsSame(model, cell.x, cell.y - 2, type))
                return true;

            if (IsSame(model, cell.x, cell.y + 1, type) && IsSame(model, cell.x, cell.y + 2, type))
                return true;

            if (IsSame(model, cell.x, cell.y - 1, type) && IsSame(model, cell.x, cell.y + 1, type))
                return true;

            return false;
        }

        // Simple near-match risk score (adjacency-based)
        private static int NearMatchScore(IGridModel model, Vector2Int cell, ItemType type)
        {
            var score = 0;

            // adjacency
            if (IsSame(model, cell.x - 1, cell.y, type)) score++;
            if (IsSame(model, cell.x + 1, cell.y, type)) score++;
            if (IsSame(model, cell.x, cell.y - 1, type)) score++;
            if (IsSame(model, cell.x, cell.y + 1, type)) score++;

            // "two-in-line potential" penalties (stronger)
            if (IsSame(model, cell.x - 2, cell.y, type) && IsSame(model, cell.x - 1, cell.y, type)) score += 4;
            if (IsSame(model, cell.x + 2, cell.y, type) && IsSame(model, cell.x + 1, cell.y, type)) score += 4;
            if (IsSame(model, cell.x - 1, cell.y, type) && IsSame(model, cell.x + 1, cell.y, type)) score += 4;

            if (IsSame(model, cell.x, cell.y - 2, type) && IsSame(model, cell.x, cell.y - 1, type)) score += 4;
            if (IsSame(model, cell.x, cell.y + 2, type) && IsSame(model, cell.x, cell.y + 1, type)) score += 4;
            if (IsSame(model, cell.x, cell.y - 1, type) && IsSame(model, cell.x, cell.y + 1, type)) score += 4;

            return score;
        }

        private static bool IsSame(IGridModel model, int x, int y, ItemType type)
        {
            if (!model.IsInRange(x, y))
                return false;

            var obj = model.GetGridObject(new Vector2Int(x, y));
            if (obj is not ItemObject item) return false;

            return item.ItemType == type;
        }

        private static ItemType[] BuildAllSpawnableTypes()
        {
            // Collect all enum values except None (0)
            var values = (ItemType[])Enum.GetValues(typeof(ItemType));

            var count = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != ItemType.None)
                    count++;
            }

            var result = new ItemType[count];
            var index = 0;

            for (int i = 0; i < values.Length; i++)
            {
                var v = values[i];
                if (v == ItemType.None)
                    continue;

                result[index++] = v;
            }

            return result;
        }
    }
}