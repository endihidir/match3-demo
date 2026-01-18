using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchBoosterDecision
    {
        public static BoosterType? DecideBoosterTypeFromGroup(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            GetGroupLineLengths(group, out var hLen, out var vLen);

            var hasSquare = Has2X2SquareInGroup(model, matchMask, group, id);

            if (hLen >= 5 || vLen >= 5)
            {
                // TODO: SELECT ORB
                return hLen >= vLen ? BoosterType.RocketVertical : BoosterType.RocketHorizontal;
            }

            if (hLen >= 3 && vLen >= 3)
            {
                return BoosterType.Bomb;
            }

            if (hLen == 4 || vLen == 4)
            {
                return hLen >= vLen ? BoosterType.RocketVertical : BoosterType.RocketHorizontal;
            }

            if (hasSquare)
            {
                // TODO: SELECT FLY
                return BoosterType.Bomb;
            }

            return null;
        }
        
        public static Vector2Int SelectMergeCenter(List<Vector2Int> group)
        {
            GetGroupBounds(group, out var minX, out var maxX, out var minY, out var maxY);

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

        private static bool Has2X2SquareInGroup(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            var set = new HashSet<Vector2Int>(group);
            var grid = model.BuildTypeDataGrid();

            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];
                
                if (!GridMatchCalc.Has2X2SquareAt(model, grid, c.x, c.y, id)) continue;

                var b = new Vector2Int(c.x + 1, c.y);
                var d = new Vector2Int(c.x, c.y + 1);
                var e = new Vector2Int(c.x + 1, c.y + 1);

                if (!set.Contains(b) || !set.Contains(d) || !set.Contains(e)) continue;

                if (!matchMask[c.x, c.y] || !matchMask[b.x, b.y] || !matchMask[d.x, d.y] || !matchMask[e.x, e.y])
                    continue;

                return true;
            }

            return false;
        }
        
        private static void GetGroupLineLengths(List<Vector2Int> group, out int hLen, out int vLen)
        {
            GetGroupBounds(group, out var minX, out var maxX, out var minY, out var maxY);

            hLen = maxX - minX + 1;
            vLen = maxY - minY + 1;
        }
        
        private static void GetGroupBounds(List<Vector2Int> group, out int minX, out int maxX, out int minY, out int maxY)
        {
            minX = int.MaxValue;
            maxX = int.MinValue;
            minY = int.MaxValue;
            maxY = int.MinValue;

            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                if (c.x < minX) minX = c.x;
                if (c.x > maxX) maxX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.y > maxY) maxY = c.y;
            }
        }
    }
}