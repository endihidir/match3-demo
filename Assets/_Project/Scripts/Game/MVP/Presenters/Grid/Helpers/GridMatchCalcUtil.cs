using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchCalcUtil
    { 
        public static bool HasAnyRegularMatchOnBoard(IGridModel model)
        {
            var grid = model.BuildGridTypeData();
            
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
        
        public static bool WouldSwapCreateMatch(IGridModel model, GridObjectType[,] grid, Vector2Int coordA, Vector2Int coordB, int typeA, int typeB)
        {
            var cellA = grid[coordA.x, coordA.y];
            var cellB = grid[coordB.x, coordB.y];

            grid[coordA.x, coordA.y] = new GridObjectType(cellA.ItemKind, typeB);
            grid[coordB.x, coordB.y] = new GridObjectType(cellB.ItemKind, typeA);

            var creates = IsCellMatched(model, grid, coordA.x, coordA.y, typeB) ||
                          IsCellMatched(model, grid, coordB.x, coordB.y, typeA);

            grid[coordA.x, coordA.y] = cellA;
            grid[coordB.x, coordB.y] = cellB;

            return creates;
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

            var hLen = 1 + left + right;
            var vLen = 1 + down + up;

            var hasH = hLen >= 3;
            var hasV = vLen >= 3;

            var maxLineLen = hLen >= vLen ? hLen : vLen;
            var lineMode = maxLineLen >= 4;

            if (lineMode)
            {
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
            }
            else
            {
                var anySquare = false;

                anySquare |= AddSquare(x, y);
                anySquare |= AddSquare(x - 1, y);
                anySquare |= AddSquare(x, y - 1);
                anySquare |= AddSquare(x - 1, y - 1);

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

                if (!anySquare && !hasH && !hasV)
                    return 0;
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

            bool AddSquare(int sx, int sy)
            {
                if (!Has2X2SquareAt(model, grid, sx, sy, id)) return false;

                var before = count;

                TryAdd(sx, sy);
                TryAdd(sx + 1, sy);
                TryAdd(sx, sy + 1);
                TryAdd(sx + 1, sy + 1);

                return count != before;
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
        
        public static bool TryBuildMatchGroupMaskAt(IGridModel model, Vector2Int targetCoord, bool[,] matchMask, out bool[,] resultMask, bool shouldBoosterResult = false)
        {
            var width = model.Width;
            var height = model.Height;

            resultMask = new bool[width, height];

            if (!model.IsInRange(targetCoord.x, targetCoord.y)) return false;
            if (matchMask == null) return false;
            if (!matchMask[targetCoord.x, targetCoord.y]) return false;

            var grid = model.BuildGridTypeData();
            var data = grid[targetCoord.x, targetCoord.y];

            if (!IsRegularItem(data)) return false;

            var id = data.TypeId;
            if (id <= 0) return false;

            var visited = new bool[width, height];
            var buffer = new Vector2Int[width * height];

            var count = CollectMatchShapeFromCenter(model, grid, targetCoord.x, targetCoord.y, id, visited, buffer);
            if (count <= 0) return false;

            if (shouldBoosterResult)
            {
                var group = new List<Vector2Int>(count);
                
                for (int i = 0; i < count; i++)
                    group.Add(buffer[i]);

                var boosterType = GridMatchBoosterDecision.DecideBoosterTypeFromGroup(model, matchMask, group, id);
                
                if (!boosterType.HasValue) return false;
            }

            for (int i = 0; i < count; i++)
            {
                var c = buffer[i];
                resultMask[c.x, c.y] = true;
            }

            return true;
        }

        private static List<Vector2Int> CollectGroupFromMask(IGridModel model, bool[,] matchMask, bool[,] visited, Vector2Int start)
        {
            var result = new List<Vector2Int>();
            var stack = new Stack<Vector2Int>();

            var startObj = model.GetGridObject(start.x, start.y);
            if (!startObj || !IsRegularItem(startObj.ObjectType)) return result;

            var typeId = startObj.TypeId;

            stack.Push(start);
            visited[start.x, start.y] = true;

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                result.Add(p);

                foreach (var dir in DirectionLookup.LinearDirections)
                {
                    if (!model.TryGetNeighbourCoord(p, dir, out var n)) continue;
                    
                    TryPush(n.x, n.y);
                }
            }

            return result;

            void TryPush(int x, int y)
            {
                if (!model.IsInRange(x, y)) return;
                if (visited[x, y]) return;
                if (!matchMask[x, y]) return;

                var obj = model.GetGridObject(x, y);
                if (!obj) return;

                if (!IsRegularItem(obj.ObjectType)) return;
                if (obj.TypeId != typeId) return;

                visited[x, y] = true;
                stack.Push(new Vector2Int(x, y));
            }
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
        
        public static bool IsAnyGroupObjectFall(IGridModel model, List<Vector2Int> group)
        {
            for (var i = 0; i < group.Count; i++)
            {
                var coord = group[i];
                var obj = model.GetGridObject(coord);
                if (obj && obj.IsActive && obj.IsFallInProgress)
                {
                    return true;
                }
            }

            return false;
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
        
        public static bool IsCellsRegular(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var isSourceRegular = IsRegularItem(sourceObj.ObjectType);
            var isTargetRegular = IsRegularItem(targetObj.ObjectType);
            return isSourceRegular && isTargetRegular;
        }

        public static bool IsRegularItem(GridObjectType data) => data is { ItemKind: GridItemKind.Regular, TypeId: > 0 };
    }
}