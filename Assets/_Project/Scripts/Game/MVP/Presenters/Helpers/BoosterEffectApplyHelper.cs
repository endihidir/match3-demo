using Core.Config;
using Core.Handlers;
using Core.Item;
using UnityEngine;

namespace Core.Utils
{
    public static class BoosterEffectApplyHelper
    {
        public static void ApplyEffect(CellResolveData[,] cells, int width, int height, Vector2Int origin, BoosterActionBase action)
        {
            switch (action)
            {
                case RocketHorizontalAction rocketH:
                    ApplyRowBandDamage(cells, width, height, origin.y, rocketH.LineCount, rocketH.DamageAmount, DamageSource.Booster);
                    return;

                case RocketVerticalAction rocketV:
                    ApplyColumnBandDamage(cells, width, height, origin.x, rocketV.LineCount, rocketV.DamageAmount, DamageSource.Booster);
                    return;

                case BombAction bomb:
                    ApplySquareDamage(cells, width, height, origin, bomb.Radius, bomb.DamageAmount, DamageSource.Booster);
                    return;

                case FullGridRemoveAction full:
                    ApplyAllDamage(cells, width, height, full.DamageAmount, DamageSource.Booster);
                    return;
                
                case OrbAction orbEffect:
                    // NOT SUPPORTED YET!
                    return;
                
                case FlyAction flyEffect:
                    // NOT SUPPORTED YET!
                    return;
            }
        }

        private static void ApplyRowBandDamage(CellResolveData[,] cells, int width, int height, int centerY, int lineCount, int damage, DamageSource source)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dy = -half; dy <= half; dy++)
            {
                var y = centerY + dy;
                if (y < 0 || y >= height) continue;

                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];
                    cell.MarkRemove(source);
                    cell.AddObstacleDamage(damage, source);
                }
            }
        }

        private static void ApplyColumnBandDamage(CellResolveData[,] cells, int width, int height, int centerX, int lineCount, int damage, DamageSource source)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dx = -half; dx <= half; dx++)
            {
                var x = centerX + dx;
                if (x < 0 || x >= width) continue;

                for (int y = 0; y < height; y++)
                {
                    ref var cell = ref cells[x, y];
                    cell.MarkRemove(source);
                    cell.AddObstacleDamage(damage, source);
                }
            }
        }

        private static void ApplySquareDamage(CellResolveData[,] cells, int width, int height, Vector2Int center, int radius, int damage, DamageSource source)
        {
            radius = Mathf.Max(0, radius);

            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;

                    ref var cell = ref cells[x, y];
                    cell.MarkRemove(source);
                    cell.AddObstacleDamage(damage, source);
                }
            }
        }

        private static void ApplyAllDamage(CellResolveData[,] cells, int width, int height, int damage, DamageSource source)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];
                    cell.MarkRemove(source);
                    cell.AddObstacleDamage(damage, source);
                }
            }
        }
    }
}