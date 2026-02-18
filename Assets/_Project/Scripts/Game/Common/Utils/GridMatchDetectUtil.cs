using Game.Grid.Item;

namespace Game.Utils
{
    public static class GridMatchDetectUtil
    {
        public static bool WouldCreateBlastGroup(GridObjectType[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            if (!assumeCenterIsId)
            {
                if (!IsRegularItem(grid[x, y])) return false;
                if (grid[x, y].TypeId != id) return false;
            }

            if (HasLineMatchAt(grid, x, y, width, height, id, assumeCenterIsId)) return true;
            if (Has2X2Square(grid, x, y, width, height, id, assumeCenterIsId)) return true;

            return false;
        }

        private static bool HasLineMatchAt(GridObjectType[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            if (!IsCenterOk(grid, x, y, id, assumeCenterIsId)) return false;

            var left = CountSame(grid, x, y, width, height, id, -1, 0);
            var right = CountSame(grid, x, y, width, height, id, 1, 0);

            if (left + 1 + right >= 3) return true;

            var down = CountSame(grid, x, y, width, height, id, 0, -1);
            var up = CountSame(grid, x, y, width, height, id, 0, 1);

            return down + 1 + up >= 3;
        }

        public static bool Has2X2Square(GridObjectType[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            if (IsSquareAt(x, y)) return true;
            if (IsSquareAt(x - 1, y)) return true;
            if (IsSquareAt(x, y - 1)) return true;
            if (IsSquareAt(x - 1, y - 1)) return true;

            return false;

            bool IsSquareAt(int sx, int sy)
            {
                if (!IsInRange(sx, sy, width, height)) return false;
                if (!IsInRange(sx + 1, sy + 1, width, height)) return false;

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

        private static bool IsCenterOk(GridObjectType[,] grid, int x, int y, int id, bool assumeCenterIsId)
        {
            if (assumeCenterIsId) return true;

            var data = grid[x, y];
            return IsRegularItem(data) && data.TypeId == id;
        }

        private static int CountSame(GridObjectType[,] grid, int x, int y, int width, int height, int id, int dx, int dy)
        {
            var count = 0;

            var cx = x + dx;
            var cy = y + dy;

            while (IsInRange(cx, cy, width, height))
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
        public static bool IsRegularItem(GridObjectType data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
        public static bool IsInRange(int x, int y, int width, int height) => x >= 0 && y >= 0 && x < width && y < height;
    }
}