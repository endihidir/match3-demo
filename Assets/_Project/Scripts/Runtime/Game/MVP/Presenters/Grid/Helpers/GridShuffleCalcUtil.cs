using Game.Grid.Item;
using Game.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridShuffleCalcUtil
    {
        public static bool HasBoosterMove(IGridModel model, GridObjectType[,] grid)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var type = grid[x, y];
                    if (type.ItemKind == GridItemKind.Booster) return true;
                }
            }

            return false;
        }
        
        public static bool HasAnyPotentialMatchGeometry(IGridModel model)
        {
            for (int y = 0; y < model.Height; y++)
            {
                if (HasRunInLine(model, 0, y, 1, 0, model.Width))
                    return true;
            }

            for (int x = 0; x < model.Width; x++)
            {
                if (HasRunInLine(model, x, 0, 0, 1, model.Height))
                    return true;
            }

            return false;
        }

        private static bool HasRunInLine(IGridModel model, int startX, int startY, int dx, int dy, int length)
        {
            var run = 0;

            for (int i = 0; i < length; i++)
            {
                var obj = model.GetGridObject(startX + dx * i, startY + dy * i);

                if (IsSwapCandidate(obj))
                {
                    run++;
                    
                    if (run >= 3) 
                        return true;
                }
                else
                {
                    run = 0;
                }
            }

            return false;
        }

        public static bool HasAnySwappableAdjacency(IGridModel model)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var a = model.GetGridObject(x, y);
                    if (!IsSwapCandidate(a)) continue;

                    if (model.IsInRange(x + 1, y))
                    {
                        var b = model.GetGridObject(x + 1, y);
                        if (IsSwapCandidate(b)) return true;
                    }

                    if (model.IsInRange(x, y + 1))
                    {
                        var b = model.GetGridObject(x, y + 1);
                        if (IsSwapCandidate(b)) return true;
                    }
                }
            }

            return false;
        }

        public static bool TrySwapCreatesMatch(IGridModel model, GridObjectType[,] grid, int ax, int ay, int bx, int by)
        {
            if (!model.IsInRange(bx, by)) return false;

            var objA = model.GetGridObject(ax, ay);
            var objB = model.GetGridObject(bx, by);

            if (!IsSwapCandidate(objA) || !IsSwapCandidate(objB)) return false;

            return GridMatchCalcUtil.IsCellsRegular(objA, objB) && 
                   GridMatchCalcUtil.WouldSwapCreateMatch(model, grid, new Vector2Int(ax, ay),new Vector2Int(bx, by), objA.TypeId, objB.TypeId);
        }

        public static bool TryFindAnyMatchedCell(IGridModel model, GridObjectType[,] grid, out Vector2Int coord)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var obj = model.GetGridObject(x, y);
                    if (!IsSwapCandidate(obj)) continue;

                    var typeId = obj.TypeId;

                    if (!GridMatchCalcUtil.IsCellMatched(model, grid, x, y, typeId)) continue;
                    
                    coord = new Vector2Int(x, y);
                    
                    return true;
                }
            }

            coord = default;
            return false;
        }

        public static void PlaceObjectAt(IGridModel model, GridObjectType[,] grid, BaseGridObject obj, Vector2Int targetCoord)
        {
            if (!obj) return;

            if (obj.Coord == targetCoord) return;

            SwapInModelAndGrid(model, grid, obj.Coord, targetCoord);
        }

        public static void SwapInModelAndGrid(IGridModel model, GridObjectType[,] grid, Vector2Int aCoord, Vector2Int bCoord)
        {
            model.Swap(aCoord, bCoord);
            SwapGridCells(grid, aCoord.x, aCoord.y, bCoord.x, bCoord.y);
        }

        public static void SwapGridCells(GridObjectType[,] grid, int ax, int ay, int bx, int by) => (grid[ax, ay], grid[bx, by]) = (grid[bx, by], grid[ax, ay]);
        public static bool IsCellUsableForMove(IGridModel model, Vector2Int coord) => TryGetUsableObject(model, coord, out _);
        public static bool TryGetUsableObject(IGridModel model, Vector2Int coord, out BaseGridObject obj)
        {
            obj = null;

            if (!model.IsInRange(coord)) return false;
            if (!model.IsCellActive(coord)) return false;

            obj = model.GetGridObject(coord);
            return IsSwapCandidate(obj);
        }

        public static bool IsSwapCandidate(BaseGridObject obj)
        {
            if (!obj) return false;
            if (obj.IsStationary) return false;
            return obj.ItemKind != GridItemKind.Booster;
        }
    }
}