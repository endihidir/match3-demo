using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseGridObject>
    {
        void Swap(Vector2Int sourceCoord, Vector2Int targetCoord);
        bool HasStationaryAndBlocking();
        int FindFallSourceY(int x, int startY);
        GridObjectType[,] BuildTypeDataGrid();

        bool TryFindVerticalSource(int x, int destY, out Vector2Int sourceCoord);
        bool TryGetBarrierYAbove(int x, int destY, out int barrierY);
        bool CanFallStraightDown(Vector2Int pos);
    }

    public class GridModel : BaseGridModel<BaseGridObject>, IGridModel
    {
        protected override void OnInitialize()
        {
        }

        public void Swap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (!IsInRange(sourceCoord) || !IsInRange(targetCoord)) return;

            var objA = GetGridObjectFast(sourceCoord.x, sourceCoord.y);
            var objB = GetGridObjectFast(targetCoord.x, targetCoord.y);

            SetGridObjectFast(sourceCoord.x, sourceCoord.y, objB);
            SetGridObjectFast(targetCoord.x, targetCoord.y, objA);
        }

        public int FindFallSourceY(int x, int startY)
        {
            if (x < 0 || x >= Width) return -1;

            for (int y = startY; y >= 0; y--)
            {
                if (!IsCellActiveFast(x, y)) continue;

                var obj = GetGridObjectFast(x, y);
                if (obj) return y;
            }

            return -1;
        }

        public bool HasStationaryAndBlocking()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var obj = GetGridObjectFast(x, y);

                    if (!obj || !obj.IsStationary) continue;
                    if (y < Height - 1) return true;
                }
            }

            return false;
        }

        public GridObjectType[,] BuildTypeDataGrid()
        {
            var grid = new GridObjectType[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var obj = GetGridObjectFast(x, y);
                    grid[x, y] = obj ? obj.ObjectType : default;
                }
            }

            return grid;
        }

        public bool TryFindVerticalSource(int x, int destY, out Vector2Int sourceCoord) => TryScanUpForSource(x, destY, out sourceCoord);
        public bool TryGetBarrierYAbove(int x, int destY, out int barrierY) => TryScanUpForBarrier(x, destY, out barrierY);

        public bool CanFallStraightDown(Vector2Int pos)
        {
            var below = new Vector2Int(pos.x, pos.y + 1);
            if (!IsInRange(below)) return false;
            if (!IsCellActiveFast(below.x, below.y)) return false;
            return !GetGridObjectFast(below.x, below.y);
        }

        private bool TryScanUpForSource(int x, int destY, out Vector2Int src)
        {
            for (int y = destY - 1; y >= 0; y--)
            {
                if (!IsCellActiveFast(x, y)) continue;

                var obj = GetGridObjectFast(x, y);
                if (!obj) continue;

                if (obj.IsStationary) break;

                src = new Vector2Int(x, y);
                return true;
            }

            src = default;
            return false;
        }

        private bool TryScanUpForBarrier(int x, int destY, out int barrierY)
        {
            for (int y = destY - 1; y >= 0; y--)
            {
                if (!IsCellActiveFast(x, y)) continue;

                var obj = GetGridObjectFast(x, y);
                if (obj && obj.IsStationary)
                {
                    barrierY = y;
                    return true;
                }
            }

            barrierY = -1;
            return false;
        }

        protected override void SetInternal(Vector2Int coord, BaseGridObject value, bool raiseEvent = true)
        {
            value?.SetCoordinate(coord);
            base.SetInternal(coord, value, raiseEvent);
        }
    }
}