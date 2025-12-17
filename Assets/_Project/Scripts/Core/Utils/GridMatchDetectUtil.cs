using Core.Item;

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
    }
}