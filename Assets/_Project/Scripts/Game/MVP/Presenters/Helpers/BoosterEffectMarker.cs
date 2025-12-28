using Core.Config;
using Core.Handlers;
using Core.Item;
using UnityEngine;

namespace Core.Utils
{
    public static class BoosterEffectMarker
    {
        private delegate void CellApply(ref CellResolveData cell, int damage, DamageSource source);

        public static void MarkObstacleDamageArea(CellResolveData[,] cells, int width, int height, Vector2Int origin, BoosterActionBase action)
        {
            ApplyInternal(cells, width, height, origin, action, ApplyDamage);
        }

        public static void MarkClearArea(CellResolveData[,] cells, int width, int height, Vector2Int origin, BoosterActionBase action)
        {
            ApplyInternal(cells, width, height, origin, action, ApplyRemove);
        }

        private static void ApplyInternal(CellResolveData[,] cells, int width, int height, Vector2Int origin, BoosterActionBase action, CellApply apply)
        {
            switch (action)
            {
                case RocketHorizontalAction rocketH:
                    ApplyRowBand(cells, width, height, origin.y, rocketH.LineCount, rocketH.DamageAmount, DamageSource.Booster, apply);
                    return;

                case RocketVerticalAction rocketV:
                    ApplyColumnBand(cells, width, height, origin.x, rocketV.LineCount, rocketV.DamageAmount, DamageSource.Booster, apply);
                    return;

                case BombAction bomb:
                    ApplySquare(cells, width, height, origin, bomb.Radius, bomb.DamageAmount, DamageSource.Booster, apply);
                    return;

                case FullGridRemoveAction full:
                    ApplyAll(cells, width, height, full.DamageAmount, DamageSource.Booster, apply);
                    return;

                case OrbAction:
                case FlyAction:
                    return;
            }
        }

        private static void ApplyRemove(ref CellResolveData cell, int damage, DamageSource source)
        {
            cell.MarkRemove();
        }

        private static void ApplyDamage(ref CellResolveData cell, int damage, DamageSource source)
        {
            cell.AddDamage(damage, source);
        }

        private static void ApplyRowBand(CellResolveData[,] cells, int width, int height, int centerY, int lineCount, int damage, DamageSource source, CellApply apply)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dy = -half; dy <= half; dy++)
            {
                var y = centerY + dy;
                if (y < 0 || y >= height) continue;

                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];
                    apply(ref cell, damage, source);
                }
            }
        }

        private static void ApplyColumnBand(CellResolveData[,] cells, int width, int height, int centerX, int lineCount, int damage, DamageSource source, CellApply apply)
        {
            var half = Mathf.Max(0, (lineCount - 1) / 2);

            for (int dx = -half; dx <= half; dx++)
            {
                var x = centerX + dx;
                if (x < 0 || x >= width) continue;

                for (int y = 0; y < height; y++)
                {
                    ref var cell = ref cells[x, y];
                    apply(ref cell, damage, source);
                }
            }
        }

        private static void ApplySquare(CellResolveData[,] cells, int width, int height, Vector2Int center, int radius, int damage, DamageSource source, CellApply apply)
        {
            radius = Mathf.Max(0, radius);

            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    if (x < 0 || y < 0 || x >= width || y >= height) continue;

                    ref var cell = ref cells[x, y];
                    apply(ref cell, damage, source);
                }
            }
        }

        private static void ApplyAll(CellResolveData[,] cells, int width, int height, int damage, DamageSource source, CellApply apply)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ref var cell = ref cells[x, y];
                    apply(ref cell, damage, source);
                }
            }
        }
    }
}
