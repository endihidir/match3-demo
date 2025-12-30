using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel : IBaseGridModel<BaseGridObject>
    {
        void Swap(Vector2Int sourceCoord, Vector2Int targetCoord);
        GridObjectType[,] BuildTypeDataGrid();
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

            SetInternal(sourceCoord, objB);
            SetInternal(targetCoord, objA);
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

        protected override void SetInternal(Vector2Int coord, BaseGridObject value, bool raiseEvent = true)
        {
            value?.SetCoordinate(coord);
            base.SetInternal(coord, value, raiseEvent);
        }
    }
}