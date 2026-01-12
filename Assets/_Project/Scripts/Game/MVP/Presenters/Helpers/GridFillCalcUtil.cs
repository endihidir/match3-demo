using Core.Handlers;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridFillCalcUtil
    {
        public static bool HasStationaryAndBlocking(IGridModel model)
        {
            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    if (!TryGetActiveObject(model, x, y, out var obj)) continue;
                    if (!obj.IsStationary) continue;

                    if (CanPassUnderStationary(model, x, y))
                        return true;
                }
            }

            return false;
        }

        private static bool CanPassUnderStationary(IGridModel model, int x, int stationaryY)
        {
            // Scan downwards in the same column.
            for (int y = stationaryY + 1; y < model.Height; y++)
            {
                var c = new Vector2Int(x, y);

                // If the column segment ends, there is nothing "under" to pass into.
                if (!model.IsCellActive(c))
                    return false;

                // Empty active cell under stationary => pass/slide is possible.
                if (!TryGetActiveObject(model, x, y, out var obj))
                    return true;

                // Non-stationary under stationary => eventually something can move / space can be created.
                if (!obj.IsStationary)
                    return true;

                // Still stationary, keep scanning.
            }

            // Reached bottom and everything below was stationary.
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
        
        public static bool HasStationaryAboveInSameSegment(IGridModel model, Vector2Int coord) => 
            TryFindFirstObjectAboveInSameSegment(model, coord, out var obj) && obj.IsStationary;

        public static bool CanFallVertically(IGridModel model, int x, int y, out Vector2Int source)
        {
            for (int sy = y - 1; sy >= 0; sy--)
            {
                var c = new Vector2Int(x, sy);

                if (!model.IsCellActive(c))
                {
                    source = default;
                    return false;
                }

                var obj = model.GetGridObject(c);

                if (!obj) continue;

                if (obj.IsStationary)
                {
                    source = default;
                    return false;
                }

                source = c;
                return true;
            }

            source = default;
            return false;
        }
        
        public static bool TryCollectDiagonalSide(IGridModel model, Vector2Int targetCoord, int dirX, out SlideDownCandidate slide)
        {
            slide = default;

            var src = new Vector2Int(targetCoord.x + dirX, targetCoord.y - 1);

            if (!model.IsInRange(src)) return false;

            if (!model.IsCellActive(src)) return false;

            var item = model.GetGridObject(src);
            
            if (!item || item.IsStationary) return false;

            if (IsBlockerSideSource(model, src, dirX))
            {
                slide = new SlideDownCandidate(item, src, targetCoord);
                return true;
            }

            if (HasStationaryAboveInSameSegment(model, src) && !HasEmptyBelowInSegment(model, src))
            {
                slide = new SlideDownCandidate(item, src, targetCoord);
                return true;
            }
            
            if (DestinationBlockedByAdjacentRoof(model, targetCoord) && IsBlockedBelow(model, src))
            {
                slide = new SlideDownCandidate(item, src, targetCoord);
                return true;
            }

            return false;
        }
        
        public static bool IsEmptyActiveCell(IGridModel model, Vector2Int coord)
        {
            if (!model.IsCellActive(coord)) return false;
            return !model.GetGridObject(coord);
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
        
        private static bool IsBlockerSideSource(IGridModel model, Vector2Int src, int dirX)
        {
            var sideOfSource = new Vector2Int(src.x - dirX, src.y);

            if (!model.IsInRange(sideOfSource)) return false;

            var sideObj = model.GetGridObject(sideOfSource);
            
            if (!sideObj || !sideObj.IsStationary) return false;

            var slideSide = new Vector2Int(src.x + dirX, src.y);

            if (model.IsInRange(slideSide))
            {
                var slideSideObj = model.GetGridObject(slideSide);
                
                if (slideSideObj && slideSideObj.IsStationary) return false;
            }

            return true;
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
                var c = new Vector2Int(src.x, sy);

                if (!model.IsCellActive(c)) return false;

                var obj = model.GetGridObject(c);

                if (obj && obj.IsStationary) return false;

                if (!obj) return true;
            }

            return false;
        }
        
        private static bool DestinationBlockedByAdjacentRoof(IGridModel model, Vector2Int targetCoord)
        {
            var roofY = targetCoord.y - 1;
            if (roofY < 0) return false;

            var lu = new Vector2Int(targetCoord.x - 1, roofY);
            if (model.IsInRange(lu) && model.IsCellActive(lu))
            {
                var o = model.GetGridObject(lu);
                if (o && o.IsStationary) return true;
            }

            var ru = new Vector2Int(targetCoord.x + 1, roofY);
            if (model.IsInRange(ru) && model.IsCellActive(ru))
            {
                var o = model.GetGridObject(ru);
                if (o && o.IsStationary) return true;
            }

            return false;
        }

        private static bool IsBlockedBelow(IGridModel model, Vector2Int c)
        {
            var below = new Vector2Int(c.x, c.y + 1);
            if (!model.IsInRange(below)) return true;
            if (!model.IsCellActive(below)) return true;

            var obj = model.GetGridObject(below);
            return obj != null;
        }
        
        private static bool TryFindFirstObjectAboveInSameSegment(IGridModel model, Vector2Int from, out BaseGridObject obj)
        {
            obj = null;

            for (int y = from.y - 1; y >= 0; y--)
            {
                var c = new Vector2Int(from.x, y);

                if (!model.IsCellActive(c)) return false;

                obj = model.GetGridObject(c);
                if (obj) return true;
            }

            return false;
        }
    }
}