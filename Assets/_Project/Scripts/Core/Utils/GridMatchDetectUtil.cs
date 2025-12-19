using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchDetectUtil
    {
        public static bool WouldCreateBlastGroup(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
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
        
        public static bool HasLineMatchAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            var center = assumeCenterIsId ? 1 : (IsRegularItem(grid[x, y]) && grid[x, y].TypeId == id ? 1 : 0);
            if (center == 0) return false;

            var left = CountSame(-1, 0);
            var right = CountSame( 1, 0);

            if (left + center + right >= 3) return true;

            var down = CountSame(0, -1);
            var up = CountSame(0, 1);

            return down + center + up >= 3;
            
            int CountSame(int dx, int dy)
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
        }
        
        public static bool Has2x2Square(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
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

        public static bool IsRegularItem(GridObjectTypeData data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
        
        public static bool[,] BuildMatchMask(GridObjectTypeData[,] grid, int width, int height, out bool anyMatch)
        {
            var remove = new bool[width, height];
            anyMatch = false;

            // Horizontal runs (>=3)
            for (int y = 0; y < height; y++)
            {
                int x = 0;

                while (x < width)
                {
                    if (!IsRegularItem(grid[x, y]))
                    {
                        x++;
                        continue;
                    }

                    var id = grid[x, y].TypeId;
                    int start = x;
                    int end = x;

                    while (end + 1 < width && IsRegularItem(grid[end + 1, y]) && grid[end + 1, y].TypeId == id)
                    {
                        end++;
                    }

                    var len = end - start + 1;

                    if (len >= 3)
                    {
                        anyMatch = true;

                        for (int cx = start; cx <= end; cx++)
                        {
                            remove[cx, y] = true;
                        }
                    }

                    x = end + 1;
                }
            }

            // Vertical runs (>=3)
            for (int x = 0; x < width; x++)
            {
                int y = 0;

                while (y < height)
                {
                    if (!IsRegularItem(grid[x, y]))
                    {
                        y++;
                        continue;
                    }

                    var id = grid[x, y].TypeId;
                    int start = y;
                    int end = y;

                    while (end + 1 < height && IsRegularItem(grid[x, end + 1]) && grid[x, end + 1].TypeId == id)
                    {
                        end++;
                    }

                    var len = end - start + 1;

                    if (len >= 3)
                    {
                        anyMatch = true;

                        for (int cy = start; cy <= end; cy++)
                        {
                            remove[x, cy] = true;
                        }
                    }

                    y = end + 1;
                }
            }

            // 2x2 squares
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    var a = grid[x, y];
                    if (!IsRegularItem(a)) continue;

                    var id = a.TypeId;

                    var b = grid[x + 1, y];
                    var c = grid[x, y + 1];
                    var d = grid[x + 1, y + 1];

                    if (!IsRegularItem(b) || b.TypeId != id) continue;
                    if (!IsRegularItem(c) || c.TypeId != id) continue;
                    if (!IsRegularItem(d) || d.TypeId != id) continue;

                    anyMatch = true;

                    remove[x, y] = true;
                    remove[x + 1, y] = true;
                    remove[x, y + 1] = true;
                    remove[x + 1, y + 1] = true;
                }
            }

            return remove;
        }
        
        public static bool HasAnyRegularMatchOnBoard(GridObjectTypeData[,] grid, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var data = grid[x, y];
                    if (!IsRegularItem(data)) continue;

                    if (WouldCreateBlastGroup(grid, x, y, width, height, data.TypeId, assumeCenterIsId: false))
                        return true;
                }
            }

            return false;
        }
    }
}
