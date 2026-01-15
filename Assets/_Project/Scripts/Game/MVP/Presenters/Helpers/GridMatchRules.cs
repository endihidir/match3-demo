using Core.Item;
using Core.Models;

namespace Core.Utils
{
    public static class GridMatchRules
    { 
        
        public static bool HasAnyRegularMatchOnBoard(IGridModel model)
        {
            var grid = model.BuildTypeDataGrid();
            
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var data = grid[x, y];
                    if (!IsRegularItem(data)) continue;
                    if (IsCellMatched(model, grid, x, y, data.TypeId)) return true;
                }
            }

            return false;
        }
        
        public static bool IsCellMatched(IGridModel model, GridObjectType[,] grid, int x, int y, int id)
        {
            if (!model.IsInRange(x, y)) return false;
            if (!IsRegularItem(grid[x, y])) return false;
            if (grid[x, y].TypeId != id) return false;

            var left = CountSame(model, grid, x, y, id, -1, 0);
            var right = CountSame(model, grid, x, y, id, 1, 0);
            if (1 + left + right >= 3) return true;

            var down = CountSame(model, grid, x, y, id, 0, -1);
            var up = CountSame(model, grid, x, y, id, 0, 1);
            if (1 + down + up >= 3) return true;

            if (Has2X2SquareAt(model, grid, x, y, id)) return true;
            if (Has2X2SquareAt(model, grid, x - 1, y, id)) return true;
            if (Has2X2SquareAt(model, grid, x, y - 1, id)) return true;
            if (Has2X2SquareAt(model, grid, x - 1, y - 1, id)) return true;

            return false;
        }

        public static bool Has2X2SquareAt(IGridModel model, GridObjectType[,] grid, int x, int y, int id)
        {
            if (x < 0 || y < 0) return false;
            if (x + 1 >= model.Width || y + 1 >= model.Height) return false;

            var a = grid[x, y];
            var b = grid[x + 1, y];
            var c = grid[x, y + 1];
            var d = grid[x + 1, y + 1];

            if (!IsRegularItem(a) || a.TypeId != id) return false;
            if (!IsRegularItem(b) || b.TypeId != id) return false;
            if (!IsRegularItem(c) || c.TypeId != id) return false;
            if (!IsRegularItem(d) || d.TypeId != id) return false;

            return true;
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
        
        public static bool IsCellsRegular(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var isSourceRegular = IsRegularItem(sourceObj.ObjectType);
            var isTargetRegular = IsRegularItem(targetObj.ObjectType);
            return isSourceRegular && isTargetRegular;
        }

        public static bool IsRegularItem(GridObjectType data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
    }
}