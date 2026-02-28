using Game.Grid.Item;
using Game.Grid.Strategies.Data;
using Game.Models;
using UnityEngine;

namespace Game.Grid.Utils
{
    public static class GridFillCalcUtil
    {
        public static bool HasStationaryWithMovableSpaceBelow(IGridModel model)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    if (!TryGetActiveObject(model, x, y, out var obj)) continue;
                    if (!obj.IsStationary) continue;
                    if (HasMovableSpaceBelow(model, x, y)) return true;
                }
            }

            return false;
        }

        private static bool HasMovableSpaceBelow(IGridModel model, int x, int stationaryY)
        {
            for (int y = stationaryY + 1; y < model.Height; y++)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;
                if (!TryGetActiveObject(model, x, y, out var obj)) return true;
                if (!obj.IsStationary) return true;
            }

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
        
        public static bool HasStationaryAboveInSameSegment(IGridModel model, Vector2Int coord, bool stopAtInactiveCell = true)
        {
            for (int y = coord.y - 1; y >= 0; y--)
            {
                var c = new Vector2Int(coord.x, y);

                if (!model.IsCellActive(c) && stopAtInactiveCell) return false;

                var obj = model.GetGridObject(c);

                if (!obj) continue;

                return obj.IsStationary;
            }

            return false;
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

        public static bool CanFallVertically(IGridModel model, int x, int y, out Vector2Int source)
        {
            for (int sy = y - 1; sy >= 0; sy--)
            {
                var coord = new Vector2Int(x, sy);

                var obj = model.GetGridObject(coord);

                if (!obj) continue;

                if (obj.IsStationary)
                {
                    source = default;
                    return false;
                }

                source = coord;
                return true;
            }

            source = default;
            return false;
        }
        
        public static bool TryCollectDiagonalSide(IGridModel model, Vector2Int targetCoord, int dirX, out SlideDownCandidate slide)
        {
            slide = default;

            if (!HasStationaryShadow(model, targetCoord)) return false;

            if (!model.TryGetNeighbourCoord(targetCoord, new Vector2Int(dirX, -1), out var sourceCoord)) return false;
            if (!model.IsCellActive(sourceCoord)) return false;

            var item = model.GetGridObject(sourceCoord);
            if (!item || item.IsStationary) return false;

            if (!HasEmptyBelowInSegment(model, sourceCoord))
            {
                slide = new SlideDownCandidate(item, sourceCoord, targetCoord);
                return true;
            }

            return false;
        }

        public static bool IsEmptyActiveCell(IGridModel model, Vector2Int coord)
        {
            if (!model.IsCellActive(coord)) return false;
            return !model.GetGridObject(coord);
        }

        public static bool TryGetSpawnCellCoord(IGridModel model, int x, out Vector2Int cellCoord)
        {
            for (int y = 0; y < model.Height; y++)
            {
                var coord = new Vector2Int(x, y);

                if (!model.IsCellActive(coord)) continue;

                cellCoord = coord;
                return true;
            }

            cellCoord = default;
            return false;
        }

        public static int CountEmptiesDown(IGridModel model, int x, int y, int height)
        {
            var count = 0;

            for (int yy = y; yy < height; yy++)
            {
                var c = new Vector2Int(x, yy);

                if (!model.IsCellActive(c)) break;
                if (model.GetGridObject(c)) break;

                count++;
            }

            return count;
        }

        private static bool HasEmptyBelowInSegment(IGridModel model, Vector2Int src)
        {
            for (int sy = src.y + 1; sy < model.Height; sy++)
            {
                var coord = new Vector2Int(src.x, sy);

                if (!model.IsCellActive(coord)) continue;

                var obj = model.GetGridObject(coord);

                if (obj && obj.IsStationary) return false;

                if (!obj) return true;
            }

            return false;
        }
        
        private static bool HasStationaryShadow(IGridModel model, Vector2Int targetCoord)
        {
            for (int y = targetCoord.y - 1; y >= 0; y--)
            {
                var c = new Vector2Int(targetCoord.x, y);

                if (!model.IsCellActive(c)) continue;

                var obj = model.GetGridObject(c);

                if (!obj) continue;

                return obj.IsStationary;
            }

            return false;
        }
    }
}