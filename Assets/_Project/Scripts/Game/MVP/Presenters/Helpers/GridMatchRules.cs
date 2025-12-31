using Core.Item;
using Core.Models;

namespace Core.Utils
{
    public static class GridMatchRules
    { 
        public static bool IsCellMatched(IGridModel model, GridObjectType[,] grid, int x, int y, int id)
        {
            if (!model.IsInRange(x, y)) return false;
            if (grid[x, y].TypeId != id) return false;

            var left = CountSameInGrid(model, grid, x, y, id, -1, 0);
            var right = CountSameInGrid(model, grid, x, y, id, 1, 0);
            if (1 + left + right >= 3) return true;

            var down = CountSameInGrid(model, grid, x, y, id, 0, -1);
            var up = CountSameInGrid(model, grid, x, y, id, 0, 1);
            if (1 + down + up >= 3) return true;

            if (Has2X2SquareAt(model, grid, x, y, id)) return true;
            if (Has2X2SquareAt(model, grid, x - 1, y, id)) return true;
            if (Has2X2SquareAt(model, grid, x, y - 1, id)) return true;
            if (Has2X2SquareAt(model, grid, x - 1, y - 1, id)) return true;

            return false;
        }
        
        public static bool HasAnyRegularMatchOnBoard(IGridModel model)
        {
            var grid = model.BuildTypeDataGrid();
            
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var data = grid[x, y];
                    if (!IsRegularItem(data)) continue;

                    if (IsCellMatched(model, grid, x, y, data.TypeId))
                        return true;
                }
            }

            return false;
        }

        private static int CountSameInGrid(IGridModel model, GridObjectType[,] grid, int x, int y, int id, int dx, int dy)
        {
            var count = 0;

            var cx = x + dx;
            var cy = y + dy;

            while (model.IsInRange(cx, cy))
            {
                if (grid[cx, cy].TypeId != id) break;
                count++;
                cx += dx;
                cy += dy;
            }

            return count;
        }

        private static bool Has2X2SquareAt(IGridModel model, GridObjectType[,] grid, int x, int y, int id)
        {
            if (x < 0 || y < 0) return false;
            if (x + 1 >= model.Width || y + 1 >= model.Height) return false;

            return grid[x, y].TypeId == id
                   && grid[x + 1, y].TypeId == id
                   && grid[x, y + 1].TypeId == id
                   && grid[x + 1, y + 1].TypeId == id;
        }

        public static void GetLineLengthsAt(IGridModel gridModel, int x, int y, int id, bool assumeCenterIsId, out int horizontal, out int vertical)
        {
            var grid = gridModel.BuildTypeDataGrid();
            GetLineLengthsAt(gridModel, grid, x, y, id, assumeCenterIsId, out horizontal, out vertical);
        }

        public static void GetLineLengthsAt(IGridModel gridModel, GridObjectType[,] grid, int x, int y, int id, bool assumeCenterIsId, out int horizontal, out int vertical)
        {
            if (!IsCenterOk(grid, x, y, id, assumeCenterIsId))
            {
                horizontal = 0;
                vertical = 0;
                return;
            }

            horizontal = 1 + CountSame(gridModel, grid, x, y, id, -1, 0) + CountSame(gridModel, grid, x, y, id, 1, 0);
            vertical = 1 + CountSame(gridModel, grid, x, y, id, 0, -1) + CountSame(gridModel, grid, x, y, id, 0, 1);
        }

        private static bool IsCenterOk(GridObjectType[,] gridTypes, int x, int y, int id, bool assumeCenterIsId)
        {
            if (assumeCenterIsId) return true;
            var data = gridTypes[x, y];
            return IsRegularItem(data) && data.TypeId == id;
        }

        private static int CountSame(IGridModel gridModel, GridObjectType[,] grid, int x, int y, int id, int dx, int dy)
        {
            var count = 0;

            var cx = x + dx;
            var cy = y + dy;

            while (gridModel.IsInRange(cx, cy))
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

        private static bool IsRegularItem(GridObjectType data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
    }
}