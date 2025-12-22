using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseItemObject>
    {
        void Swap(Vector2Int a, Vector2Int b);
        bool HasStationaryAndBlocking();
        int FindFallSourceY(int x, int startY);
        GridObjectTypeData[,] BuildTypeDataGrid();

        bool TryFindVerticalSource(int x, int destY, out Vector2Int src);
        bool TryGetBarrierYAbove(int x, int destY, out int barrierY);
        bool CanFallStraightDown(Vector2Int pos);
    }

    public class GridModel : BaseGridModel<BaseItemObject>, IGridModel
    {
        protected override void OnInitialize()
        {
        }

        public void Swap(Vector2Int a, Vector2Int b)
        {
            if (!IsInRange(a) || !IsInRange(b)) return;

            var objA = GetGridObject(a);
            var objB = GetGridObject(b);

            SetGridObject(a, objB);
            SetGridObject(b, objA);
        }

        public int FindFallSourceY(int x, int startY)
        {
            if (x < 0 || x >= Width) return -1;

            for (int y = startY; y >= 0; y--)
            {
                if (!ActiveCells[x, y]) continue;

                var obj = GetGridObject(new Vector2Int(x, y));
                
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
                    var obj = GetGridObject(new Vector2Int(x, y));
                    if (!obj || !obj.IsStationary) continue;
                    if (y < Height - 1) return true;
                }
            }

            return false;
        }

        public GridObjectTypeData[,] BuildTypeDataGrid()
        {
            var grid = new GridObjectTypeData[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var obj = GetGridObject(new Vector2Int(x, y));
                    grid[x, y] = obj ? obj.TypeData : default;
                }
            }

            return grid;
        }

        public bool TryFindVerticalSource(int x, int destY, out Vector2Int src)
        {
            for (int y = destY - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);

                if (!IsCellActive(pos)) continue;

                var obj = GetGridObject(pos);
                if (!obj) continue;

                if (obj.IsStationary) break;

                src = pos;
                return true;
            }

            src = default;
            return false;
        }

        public bool TryGetBarrierYAbove(int x, int destY, out int barrierY)
        {
            for (int y = destY - 1; y >= 0; y--)
            {
                var pos = new Vector2Int(x, y);
                if (!IsCellActive(pos)) continue;

                var obj = GetGridObject(pos);
                if (obj && obj.IsStationary)
                {
                    barrierY = y;
                    return true;
                }
            }

            barrierY = -1;
            return false;
        }

        public bool CanFallStraightDown(Vector2Int pos)
        {
            var below = new Vector2Int(pos.x, pos.y + 1);
            if (below.y >= Height) return false;
            if (!IsCellActive(below)) return false;

            return !GetGridObject(below);
        }
    }
}