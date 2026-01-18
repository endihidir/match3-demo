using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchCalc
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
        
        public static int CollectMatchShapeFromCenter(IGridModel model, GridObjectType[,] grid, int x, int y, int id, bool[,] visited, Vector2Int[] buffer)
        {
            var count = 0;

            if (!model.IsInRange(x, y)) return 0;
            if (!IsRegularItem(grid[x, y])) return 0;
            if (grid[x, y].TypeId != id) return 0;

            var left = CountSame(model, grid, x, y, id, -1, 0);
            var right = CountSame(model, grid, x, y, id, 1, 0);
            var down = CountSame(model, grid, x, y, id, 0, -1);
            var up = CountSame(model, grid, x, y, id, 0, 1);

            var hasH = 1 + left + right >= 3;
            var hasV = 1 + down + up >= 3;
            var hasLine = hasH || hasV;

            if (hasH)
            {
                AddLine(-1, 0, left);
                AddLine(1, 0, right);
            }

            if (hasV)
            {
                AddLine(0, -1, down);
                AddLine(0, 1, up);
            }

            if (!hasLine)
            {
                AddSquare(x, y);
                AddSquare(x - 1, y);
                AddSquare(x, y - 1);
                AddSquare(x - 1, y - 1);
            }

            TryAdd(x, y);

            return count;

            void AddLine(int dx, int dy, int len)
            {
                int cx = x;
                int cy = y;

                for (int i = 0; i < len; i++)
                {
                    cx += dx;
                    cy += dy;
                    TryAdd(cx, cy);
                }
            }

            void AddSquare(int sx, int sy)
            {
                if (!Has2X2SquareAt(model, grid, sx, sy, id)) return;

                TryAdd(sx, sy);
                TryAdd(sx + 1, sy);
                TryAdd(sx, sy + 1);
                TryAdd(sx + 1, sy + 1);
            }
            
            void TryAdd(int ax, int ay)
            {
                if (!model.IsInRange(ax, ay)) return;
                if (visited[ax, ay]) return;

                for (int i = 0; i < count; i++)
                {
                    if (buffer[i].x == ax && buffer[i].y == ay)
                        return;
                }

                buffer[count++] = new Vector2Int(ax, ay);
            }
        }

        public static int CountSame(IGridModel gridModel, GridObjectType[,] grid, int x, int y, int id, int dx, int dy)
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
        
        public static BaseGridObject[] GetMergedGroupObject(List<Vector2Int> group, IGridModel model, Vector2Int centerCoord)
        {
            var mergeObjs = new BaseGridObject[group.Count - 1];
            var index = 0;

            foreach (var coord in group)
            {
                if (coord == centerCoord) continue;
                var obj = model.GetGridObject(coord);
                if (!obj) continue;
                mergeObjs[index++] = obj;
            }

            return mergeObjs;
        }

        public static void SetNullMergedObjectCoords(IGridModel model, List<Vector2Int> group, Vector2Int centerCoord)
        {
            foreach (var coord in group.Where(coord => coord != centerCoord))
            {
                model.SetGridObject(coord, null);
            }
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