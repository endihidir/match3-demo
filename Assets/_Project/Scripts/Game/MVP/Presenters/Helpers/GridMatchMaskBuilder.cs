using Core.Item;
using Core.Models;

namespace Core.Utils
{
    public static class GridMatchMaskBuilder
    {
        public static bool[,] BuildMatchMask(IGridModel gridModel, out bool anyMatch)
        {
            var grid = gridModel.BuildTypeDataGrid();
            return BuildMatchMask(gridModel, grid, out anyMatch);
        }
        
        private static bool[,] BuildMatchMask(IGridModel gridModel, GridObjectType[,] grid, out bool anyMatch)
        {
            var remove = new bool[gridModel.Width, gridModel.Height];
            var any = false;

            ScanRuns(gridModel, grid, remove, dx: 1, dy: 0, ref any);
            ScanRuns(gridModel, grid, remove, dx: 0, dy: 1, ref any);
            ScanSquares(gridModel, grid, remove, ref any);

            anyMatch = any;
            return remove;
        }

        private static void ScanRuns(IGridModel gridModel, GridObjectType[,] grid, bool[,] remove, int dx, int dy, ref bool any)
        {
            if (dx == 1)
            {
                for (int y = 0; y < gridModel.Height; y++)
                    ScanLine(gridModel, grid, remove, startX: 0, startY: y, dx, dy, ref any);
            }
            else
            {
                for (int x = 0; x < gridModel.Width; x++)
                    ScanLine(gridModel, grid, remove, startX: x, startY: 0, dx, dy, ref any);
            }
        }

        private static void ScanLine(IGridModel gridModel, GridObjectType[,] grid, bool[,] remove, int startX, int startY, int dx, int dy, ref bool any)
        {
            var x = startX;
            var y = startY;

            while (gridModel.IsInRange(x, y))
            {
                if (!GridMatchRules.IsRegularItem(grid[x, y]))
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

                while (gridModel.IsInRange(nx, ny))
                {
                    var next = grid[nx, ny];

                    if (!GridMatchRules.IsRegularItem(next) || next.TypeId != id) break;

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

        private static void ScanSquares(IGridModel gridModel, GridObjectType[,] grid, bool[,] remove, ref bool any)
        {
            for (int y = 0; y < gridModel.Height - 1; y++)
            {
                for (int x = 0; x < gridModel.Width - 1; x++)
                {
                    var a = grid[x, y];
                    if (!GridMatchRules.IsRegularItem(a)) continue;

                    var id = a.TypeId;

                    var b = grid[x + 1, y];
                    var c = grid[x, y + 1];
                    var d = grid[x + 1, y + 1];

                    if (!GridMatchRules.IsRegularItem(b) || b.TypeId != id) continue;
                    if (!GridMatchRules.IsRegularItem(c) || c.TypeId != id) continue;
                    if (!GridMatchRules.IsRegularItem(d) || d.TypeId != id) continue;

                    any = true;

                    remove[x, y] = true;
                    remove[x + 1, y] = true;
                    remove[x, y + 1] = true;
                    remove[x + 1, y + 1] = true;
                }
            }
        }
    }
}