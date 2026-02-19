using System;
using Game.Configs;
using Game.Grid.Item;
using Game.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridSpawnPickerUtil
    {
        public static ItemType PickBest(IGridModel model, Vector2Int cell, Span<ItemType> types, int count, FillSpawnDecisionConfigSO config, Func<float> rnd01)
        {
            var best = types[0];
            var bestScore = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var type = types[i];

                var score = 0f;

                // Penalize near-match setups (adjacency / 2-in-a-row potential)
                if (config.NearMatchAvoidance > 0f)
                {
                    var nearAvoid01 = config.NearMatchAvoidance / 100f;
                    score += nearAvoid01 * NearMatchScore(model, cell, type);
                }

                // Small noise to avoid always picking the same type
                score += rnd01() * 0.01f;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = type;
                }
            }

            return best;
        }

        // Immediate match check (left2 / down2)
        public static bool CreatesImmediateMatch(IGridModel model, Vector2Int cell, ItemType type)
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
        public static int NearMatchScore(IGridModel model, Vector2Int cell, ItemType type)
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

        public static bool IsSame(IGridModel model, int x, int y, ItemType type)
        {
            if (!model.IsInRange(x, y))
                return false;

            var obj = model.GetGridObject(new Vector2Int(x, y));
            if (obj is not ItemObject item) return false;

            return item.ItemType == type;
        }

        public static ItemType[] BuildAllSpawnableTypes()
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