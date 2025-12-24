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
            var right = CountSame(1, 0);

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
            if (IsSquareAt(x, y)) return true;
            if (IsSquareAt(x - 1, y)) return true;
            if (IsSquareAt(x, y - 1)) return true;
            if (IsSquareAt(x - 1, y - 1)) return true;

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

        public static bool[,] BuildMatchMask(GridObjectTypeData[,] grid, int width, int height, out bool anyMatch)
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

        public static bool[,] BuildMatchMaskFast(GridObjectTypeData[,] grid, int width, int height, out bool anyMatch)
        {
            var remove = new bool[width, height];
            var any = false;

            ScanRuns(grid, remove, width, height, dx: 1, dy: 0, ref any);
            ScanRuns(grid, remove, width, height, dx: 0, dy: 1, ref any);
            ScanSquares(grid, remove, width, height, ref any);

            anyMatch = any;
            return remove;
        }

        private static void ScanRuns(GridObjectTypeData[,] grid, bool[,] remove, int width, int height, int dx, int dy, ref bool any)
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

        private static void ScanLine(GridObjectTypeData[,] grid, bool[,] remove, int width, int height, int startX, int startY, int dx, int dy, ref bool any)
        {
            int x = startX;
            int y = startY;

            while (x >= 0 && x < width && y >= 0 && y < height)
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

                while (nx >= 0 && nx < width && ny >= 0 && ny < height)
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

        private static void ScanSquares(GridObjectTypeData[,] grid, bool[,] remove, int width, int height, ref bool any)
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

        public static void GetLineLengthsAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId, out int horizontal, out int vertical)
        {
            var centerOk = assumeCenterIsId || (IsRegularItem(grid[x, y]) && grid[x, y].TypeId == id);
            if (!centerOk)
            {
                horizontal = 0;
                vertical = 0;
                return;
            }

            horizontal = 1 + CountSame(-1, 0) + CountSame(1, 0);
            vertical = 1 + CountSame(0, -1) + CountSame(0, 1);

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

        public static bool HasTOrLAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            GetLineLengthsAt(grid, x, y, width, height, id, assumeCenterIsId, out var h, out var v);
            return h >= 3 && v >= 3;
        }

        public static bool HasFiveLineAt(GridObjectTypeData[,] grid, int x, int y, int width, int height, int id, bool assumeCenterIsId)
        {
            GetLineLengthsAt(grid, x, y, width, height, id, assumeCenterIsId, out var h, out var v);
            return h >= 5 || v >= 5;
        }

        public static bool IsRegularItem(GridObjectTypeData data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
    }
}