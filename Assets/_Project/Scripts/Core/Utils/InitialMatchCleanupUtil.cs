using System;
using Core.Item;

namespace Core.Utils
{
    public static class InitialMatchCleanupUtil
    {
        public static void RemoveInitialMatches(GridObjectTypeData[,] grid, bool[,] randomMask, int width, int height, Random rng)
        {
            bool found;

            do
            {
                found = false;

                for (int i = 0; i < width * height; i++)
                {
                    var coord = GridIndexUtil.ToCoord(i, width);
                    var x = coord.x;
                    var y = coord.y;
            
                    if (!randomMask[x, y]) continue;
                    if (!IsRegularItem(grid[x, y])) continue;

                    if (WouldCreateBlastGroup(grid, x, y, width, height, grid[x, y].TypeId, assumeCenterIsId: false))
                    {
                        grid[x, y] = CreateRandomNonGroupingCell(grid, x, y, width, height, rng);
                        found = true;
                    }
                }

            } while (found);
        }

        private static GridObjectTypeData CreateRandomNonGroupingCell(GridObjectTypeData[,] grid, int x, int y, int width, int height, Random rng)
        {
            var ids = LevelGridRandomUtil.GetItemTypeIds();

            Span<int> candidates = stackalloc int[ids.Length];
            var candidateCount = 0;

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];

                if (!WouldCreateBlastGroup(grid, x, y, width, height, id, assumeCenterIsId: true))
                {
                    candidates[candidateCount] = id;
                    candidateCount++;
                }
            }

            if (candidateCount > 0)
            {
                var pick = LevelGridRandomUtil.NextIndex(rng, candidateCount);
                return new GridObjectTypeData(GridItemKind.Regular, candidates[pick]);
            }
            
            var attempts = 0;

            while (true)
            {
                var id = ids[LevelGridRandomUtil.NextIndex(rng, ids.Length)];

                if (!WouldCreateBlastGroup(grid, x, y, width, height, id, assumeCenterIsId: true))
                    return new GridObjectTypeData(GridItemKind.Regular, id);

                attempts++;

                if (attempts > 64)
                    return new GridObjectTypeData(GridItemKind.Regular, id);
            }
        }

        private static bool WouldCreateBlastGroup(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            if (!assumeCenterIsId)
            {
                if (!IsRegularItem(grid[x, y])) return false;
                if (grid[x, y].TypeId != id) return false;
            }
            
            if (HasLineMatchAt(grid, x, y, width, height, id, assumeCenterIsId)) return true;
            
            if (Has2x2Square(grid, x, y, width, height, id, assumeCenterIsId)) return true;

            return false;
        }
        
        private static bool HasLineMatchAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            var center = assumeCenterIsId ? 1 : (IsRegularItem(grid[x, y]) && grid[x, y].TypeId == id ? 1 : 0);
            if (center == 0) return false;

            var left = CountSame(grid, x, y, -1, 0, width, height, id);
            var right = CountSame(grid, x, y, 1, 0, width, height, id);

            if (left + center + right >= 3) return true;

            var down = CountSame(grid, x, y, 0, -1, width, height, id);
            var up = CountSame(grid, x, y, 0, 1, width, height, id);

            return down + center + up >= 3;
        }

        private static int CountSame(GridObjectTypeData[,] grid, int x, int y, int dx, int dy, int width, int height, int id)
        {
            var count = 0;

            var cx = x + dx;
            var cy = y + dy;

            while (cx >= 0 && cx < width && cy >= 0 && cy < height)
            {
                var data = grid[cx, cy];

                if (!IsRegularItem(data))
                    break;

                if (data.TypeId != id)
                    break;

                count++;

                cx += dx;
                cy += dy;
            }

            return count;
        }
        
        private static bool Has2x2Square(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            if (IsSquareAt(x, y)) return true;         // (x,y) top-left
            if (IsSquareAt(x - 1, y)) return true;     // (x,y) top-right
            if (IsSquareAt(x, y - 1)) return true;     // (x,y) bottom-left
            if (IsSquareAt(x - 1, y - 1)) return true; // (x,y) bottom-right

            return false;

            bool IsSquareAt(int sx, int sy)
            {
                if (sx < 0 || sy < 0 || sx + 1 >= width || sy + 1 >= height)
                    return false;

                if (!IsCellId(sx, sy)) return false;
                if (!IsCellId(sx + 1, sy)) return false;
                if (!IsCellId(sx, sy + 1)) return false;
                if (!IsCellId(sx + 1, sy + 1)) return false;

                return true;
            }

            bool IsCellId(int cx, int cy)
            {
                if (assumeCenterIsId && cx == x && cy == y)
                    return true;

                var data = grid[cx, cy];
                return IsRegularItem(data) && data.TypeId == id;
            }
        }

        private static bool IsRegularItem(GridObjectTypeData data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
    }
}
