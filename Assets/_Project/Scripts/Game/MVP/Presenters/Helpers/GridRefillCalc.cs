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
                    var obj = model.GetGridObjectFast(x, y);
                    if (!obj || !obj.IsStationary)
                        continue;

                    // Stationary at bottom cannot block anything
                    if (y >= model.Height - 1)
                        continue;

                    int gapY = y + 1;

                    // If the cell directly below is not active, there is no "blocked gap" to fill
                    if (!model.IsCellActiveFast(x, gapY))
                        continue;

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

        public static bool CanColumnStructurallyDonateAtRow(IGridModel model, int donorX, int rowY)
        {
            // Out of bounds
            if (donorX < 0 || donorX >= model.Width)
                return false;

            // Donor cell at the stationary row must exist (active)
            if (!model.IsCellActiveFast(donorX, rowY))
                return false;

            // Donor column must be structurally continuous (no inactive holes) from top to rowY
            // This encodes "it can be filled up to the stationary row" regardless of current occupancy.
            for (int y = 0; y <= rowY; y++)
            {
                if (!model.IsCellActiveFast(donorX, y))
                    return false;
            }

            return true;
        }

        public static bool CanDonateIntoBlockedCell(IGridModel model, int blockedX, int blockedY, int stationaryY)
        {
            // Donation is allowed only from the left or right neighbor of the stationary column
            if (CanDonateFromSide(model, blockedX + 1, blockedY, stationaryY))
                return true;

            if (CanDonateFromSide(model, blockedX - 1, blockedY, stationaryY))
                return true;

            return false;
        }

        public static bool CanDonateFromSide(IGridModel model, int donorX, int donorY, int stationaryY)
        {
            // Out of grid bounds
            if (donorX < 0 || donorX >= model.Width)
                return false;

            // Donor cell must be active
            if (!model.IsCellActiveFast(donorX, donorY))
                return false;

            var donorObj = model.GetGridObjectFast(donorX, donorY);

            // Donor must exist on the same row
            if (!donorObj)
                return false;

            // Stationary items cannot act as donors
            if (donorObj.IsStationary)
                return false;

            // Donation readiness rule:
            // The donor column must be fully settled from the top (y=0) down to the stationary Y.
            if (!IsColumnFilledUpToY(model, donorX, stationaryY))
                return false;

            return true;
        }

        public static bool IsColumnFilledUpToY(IGridModel model, int x, int yMaxInclusive)
        {
            // Y = 0 is the top.
            // Require the donor column to have no empty active cells down to stationaryY.
            for (int y = 0; y <= yMaxInclusive; y++)
            {
                if (!model.IsCellActiveFast(x, y))
                    continue;

                if (!model.GetGridObjectFast(x, y))
                    return false;
            }

            return true;
        }

        public static int FindFallSourceY(IGridModel model, int x, int startY)
        {
            // Scan upward (decreasing Y) for the first active cell that has an object.
            if (x < 0 || x >= model.Width)
                return -1;

            for (int y = startY; y >= 0; y--)
            {
                if (!model.IsCellActiveFast(x, y))
                    continue;

                var obj = model.GetGridObjectFast(x, y);
                if (obj)
                    return y;
            }

            return -1;
        }

        public static bool CanFallStraightDown(IGridModel model, Vector2Int pos)
        {
            // Falling direction is +Y (Y increases downward)
            int belowY = pos.y + 1;

            if (belowY < 0 || belowY >= model.Height)
                return false;

            if (!model.IsCellActiveFast(pos.x, belowY))
                return false;

            return !model.GetGridObjectFast(pos.x, belowY);
        }

        public static bool TryFindVerticalSource(IGridModel model, int x, int destY, out Vector2Int sourceCoord)
        {
            // Scan upward (decreasing Y) until we find a movable object.
            // Stationary blocks stop the scan.
            for (int y = destY - 1; y >= 0; y--)
            {
                if (!model.IsCellActiveFast(x, y))
                    continue;

                var obj = model.GetGridObjectFast(x, y);
                if (!obj)
                    continue;

                if (obj.IsStationary)
                    break;

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
                if (!model.IsCellActiveFast(x, y)) continue;

                var obj = model.GetGridObjectFast(x, y);
                if (obj && obj.IsStationary)
                {
                    barrierY = y;
                    return true;
                }
            }

            barrierY = -1;
            return false;
        }
        
        public static bool TryGetSpawnCell(IGridModel model, int x, int height, out Vector2Int spawnCell)
        {
            // Scan from top to bottom (y = 0 is top)
            for (int y = 0; y < height; y++)
            {
                if (!model.IsCellActiveFast(x, y))
                    continue;

                spawnCell = new Vector2Int(x, y);
                return true;
            }

            spawnCell = default;
            return false;
        }
    }
}