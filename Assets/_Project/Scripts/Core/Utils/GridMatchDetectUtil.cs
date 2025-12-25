using Core.Item;

namespace Core.Utils
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
            if (Has2x2Square(grid, x, y, width, height, id, assumeCenterIsId)) return true;

            return false;
        }

        public static bool HasLineMatchAt(GridObjectType[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            if (!IsCenterOk(grid, x, y, id, assumeCenterIsId)) return false;

            var left = CountSame(grid, x, y, width, height, id, -1, 0);
            var right = CountSame(grid, x, y, width, height, id, 1, 0);

            if (left + 1 + right >= 3) return true;

            var down = CountSame(grid, x, y, width, height, id, 0, -1);
            var up = CountSame(grid, x, y, width, height, id, 0, 1);

            return down + 1 + up >= 3;
        }

        public static bool Has2x2Square(GridObjectType[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
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

        public static bool HasAnyRegularMatchOnBoard(GridObjectType[,] grid, int width, int height)
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

        public static bool[,] BuildMatchMask(GridObjectType[,] grid, int width, int height, out bool anyMatch)
        {
            var remove = new bool[width, height];
            anyMatch = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var data = grid[x, y];
                    if (!IsRegularItem(data)) continue;

                    var id = data.TypeId;

                    if (HasLineMatchAt(grid, x, y, width, height, id, assumeCenterIsId: false) ||
                        Has2x2Square(grid, x, y, width, height, id, assumeCenterIsId: false))
                    {
                        remove[x, y] = true;
                        anyMatch = true;
                    }
                }
            }

            return remove;
        }

        public static bool[,] BuildMatchMaskFast(GridObjectType[,] grid, int width, int height, out bool anyMatch)
        {
            var remove = new bool[width, height];
            var any = false;

            ScanRuns(grid, remove, width, height, dx: 1, dy: 0, ref any);
            ScanRuns(grid, remove, width, height, dx: 0, dy: 1, ref any);
            ScanSquares(grid, remove, width, height, ref any);

            anyMatch = any;
            return remove;
        }

        private static void ScanRuns(GridObjectType[,] grid, bool[,] remove, int width, int height, int dx, int dy, ref bool any)
        {
            if (dx == 1)
            {
                for (int y = 0; y < height; y++)
                    ScanLine(grid, remove, width, height, startX: 0, startY: y, dx, dy, ref any);
            }
            else
            {
                for (int x = 0; x < width; x++)
                    ScanLine(grid, remove, width, height, startX: x, startY: 0, dx, dy, ref any);
            }
        }

        private static void ScanLine(GridObjectType[,] grid, bool[,] remove, int width, int height, int startX, int startY, int dx, int dy, ref bool any)
        {
            int x = startX;
            int y = startY;

            while (IsInRange(x, y, width, height))
            {
                if (!IsRegularItem(grid[x, y]))
                {
                    x += dx;
                    y += dy;
                    continue;
                }

                var id = grid[x, y].TypeId;

                int runStartX = x;
                int runStartY = y;
                int runLen = 1;

                int nx = x + dx;
                int ny = y + dy;

                while (IsInRange(nx, ny, width, height))
                {
                    var next = grid[nx, ny];
                    if (!IsRegularItem(next) || next.TypeId != id)
                        break;

                    runLen++;
                    nx += dx;
                    ny += dy;
                }

                if (runLen >= 3)
                {
                    any = true;

                    int cx = runStartX;
                    int cy = runStartY;

                    for (int i = 0; i < runLen; i++)
                    {
                        remove[cx, cy] = true;
                        cx += dx;
                        cy += dy;
                    }
                }

                x = nx;
                y = ny;
            }
        }

        private static void ScanSquares(GridObjectType[,] grid, bool[,] remove, int width, int height, ref bool any)
        {
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

                    any = true;

                    remove[x, y] = true;
                    remove[x + 1, y] = true;
                    remove[x, y + 1] = true;
                    remove[x + 1, y + 1] = true;
                }
            }
        }

        public static void GetLineLengthsAt(GridObjectType[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId, out int horizontal, out int vertical)
        {
            if (!IsCenterOk(grid, x, y, id, assumeCenterIsId))
            {
                horizontal = 0;
                vertical = 0;
                return;
            }

            horizontal = 1 + CountSame(grid, x, y, width, height, id, -1, 0) + CountSame(grid, x, y, width, height, id, 1, 0);
            vertical = 1 + CountSame(grid, x, y, width, height, id, 0, -1) + CountSame(grid, x, y, width, height, id, 0, 1);
        }

        public static bool IsRegularItem(GridObjectType data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };

        public static bool IsInRange(int x, int y, int width, int height) => x >= 0 && y >= 0 && x < width && y < height;

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
    }
}