using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridRefillCalc
    {
        public static bool HasStationaryAndBlocking(IGridModel model)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    if (!TryGetActiveObject(model, x, y, out var obj)) continue;
                    if (!obj.IsStationary) continue;

                    // Stationary at bottom cannot block anything
                    if (y >= model.Height - 1) continue;

                    int gapY = y + 1;

                    // If the cell directly below is not active, there is no "blocked gap" to fill
                    if (!model.IsCellActive(new Vector2Int(x, gapY))) continue;

                    // Strategy is only meaningful if at least one side column can structurally donate at row y
                    if (CanColumnStructurallyDonateAtRow(model, x - 1, y) ||
                        CanColumnStructurallyDonateAtRow(model, x + 1, y))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CanColumnStructurallyDonateAtRow(IGridModel model, int donorX, int rowY)
        {
            // Out of bounds
            if (donorX < 0 || donorX >= model.Width) return false;

            // Donor cell at the stationary row must exist (active)
            if (!model.IsCellActive(new Vector2Int(donorX, rowY))) return false;

            // Donor column must be structurally continuous (no inactive holes) from top to rowY
            // This encodes "it can be filled up to the stationary row" regardless of current occupancy.
            for (int y = 0; y <= rowY; y++)
            {
                if (!model.IsCellActive(new Vector2Int(donorX, y))) return false;
            }

            return true;
        }

        public static bool HasAnyEmptyActiveCell(IGridModel model)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var cell = new Vector2Int(x, y);

                    if (!model.IsCellActive(cell)) continue;
                    if (!model.GetGridObject(cell)) return true;
                }
            }

            return false;
        }

        public static bool IsEmptyActiveCell(IGridModel model, Vector2Int coord)
        {
            if (!model.IsCellActive(coord)) return false;
            return !model.GetGridObject(coord);
        }

        public static bool IsBarrierAtColumnTop(IGridModel model, int x, int barrierY)
        {
            for (int y = barrierY - 1; y >= 0; y--)
            {
                var cell = new Vector2Int(x, y);
                if (!model.IsCellActive(cell)) continue;
                return false;
            }

            return true;
        }

        public static bool CanFallStraightDown(IGridModel model, Vector2Int coord)
        {
            // Falling direction is +Y (Y increases downward)
            int belowY = coord.y + 1;

            if (belowY < 0 || belowY >= model.Height) return false;

            var belowCoord = new Vector2Int(coord.x, belowY);

            if (!model.IsCellActive(belowCoord)) return false;

            return !model.GetGridObject(belowCoord);
        }

        public static bool TryFindVerticalSource(IGridModel model, int x, int destY, out Vector2Int sourceCoord)
        {
            // Scan upward (decreasing Y) until we find a movable object.
            // Stationary blocks stop the scan.
            for (int y = destY - 1; y >= 0; y--)
            {
                if (!TryGetActiveObject(model, x, y, out var obj)) continue;

                if (obj.IsStationary) break;

                sourceCoord = new Vector2Int(x, y);
                return true;
            }

            sourceCoord = default;
            return false;
        }

        public static bool TryGetBarrierYAbove(IGridModel model, int x, int destY, out int barrierY)
        {
            // Scan upward (decreasing Y) for the first stationary object.
            for (int y = destY - 1; y >= 0; y--)
            {
                if (!TryGetActiveObject(model, x, y, out var obj)) continue;

                if (obj.IsStationary)
                {
                    barrierY = y;
                    return true;
                }
            }

            barrierY = -1;
            return false;
        }

        public static bool TryGetSpawnCellCoord(IGridModel model, int x, int height, out Vector2Int cellCoord)
        {
            // Scan from top to bottom (y = 0 is top)
            for (int y = 0; y < height; y++)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord))
                    continue;

                cellCoord = coord;
                return true;
            }

            cellCoord = default;
            return false;
        }

        public static int FindFallSourceY(IGridModel model, int x, int startY)
        {
            // Scan upward (decreasing Y) for the first active cell that has an object.
            if (x < 0 || x >= model.Width) return -1;

            for (int y = startY; y >= 0; y--)
            {
                if (!TryGetActiveObject(model, x, y, out _)) continue;
                return y;
            }

            return -1;
        }

        private static bool TryGetActiveObject(IGridModel model, int x, int y, out BaseGridObject obj)
        {
            obj = null;

            if (!model.IsInRange(x, y)) return false;

            var coord = new Vector2Int(x, y);

            if (!model.IsCellActive(coord)) return false;

            obj = model.GetGridObject(coord);
            
            return obj;
        }
    }
}