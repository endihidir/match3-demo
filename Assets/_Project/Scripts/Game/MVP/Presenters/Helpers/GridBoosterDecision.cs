using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridBoosterDecision
    {
        public static Vector2Int SelectMergeCenter(List<Vector2Int> group)
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            var cx = (minX + maxX) / 2;
            var cy = (minY + maxY) / 2;

            var best = group[0];
            var bestDist = int.MaxValue;

            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                var dx = c.x - cx;
                var dy = c.y - cy;

                var d = dx * dx + dy * dy;

                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }

            return best;
        }

        public static Vector2Int SelectMergeCenter(List<Vector2Int> group, bool hasForcedCoord, Vector2Int forcedCoord)
        {
            if (hasForcedCoord)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    if (group[i] == forcedCoord)
                        return forcedCoord;
                }
            }

            return SelectMergeCenter(group);
        }

        public static BoosterType? DecideBoosterTypeFromGroup(IGridModel model, GridObjectType[,] grid, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            GetGroupLineLengths(group, out var hLen, out var vLen, out var isCross);

            var hasSquare = Has2X2SquareInGroup(model, grid, matchMask, group, id);

            if (hLen >= 5 || vLen >= 5)
            {
                // TODO: SELECT ORB
                return hLen >= vLen ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;
            }

            if (isCross)
            {
                return BoosterType.Bomb;
            }

            if (hLen == 4 || vLen == 4)
            {
                return hLen >= vLen ? BoosterType.RocketHorizontal : BoosterType.RocketVertical;
            }

            if (hasSquare)
            {
                // TODO: SELECT FLY
                return BoosterType.Bomb;
            }

            return null;
        }

        private static bool Has2X2SquareInGroup(IGridModel model, GridObjectType[,] grid, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            var set = new HashSet<Vector2Int>(group);

            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                var b = new Vector2Int(c.x + 1, c.y);
                var d = new Vector2Int(c.x, c.y + 1);
                var e = new Vector2Int(c.x + 1, c.y + 1);

                if (!model.IsInRange(b) || !model.IsInRange(d) || !model.IsInRange(e)) continue;

                if (!set.Contains(b) || !set.Contains(d) || !set.Contains(e)) continue;

                if (!matchMask[c.x, c.y] || !matchMask[b.x, b.y] || !matchMask[d.x, d.y] || !matchMask[e.x, e.y]) continue;

                if (grid[c.x, c.y].TypeId != id) continue;
                if (grid[b.x, b.y].TypeId != id) continue;
                if (grid[d.x, d.y].TypeId != id) continue;
                if (grid[e.x, e.y].TypeId != id) continue;

                return true;
            }

            return false;
        }

        private static void GetGroupLineLengths(List<Vector2Int> group, out int hLen, out int vLen, out bool isCross)
        {
            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }

            hLen = maxX - minX + 1;
            vLen = maxY - minY + 1;

            isCross = hLen >= 3 && vLen >= 3;
        }
    }
}