using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseItemObject>
    {
        void Swap(Vector2Int a, Vector2Int b);
        int FindFallSourceY(int x, int startY);
        GridObjectTypeData[,] BuildTypeDataGrid();
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
    }
}