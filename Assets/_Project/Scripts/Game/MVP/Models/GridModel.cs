using Core.Item;
using UnityEngine;

namespace Core.Models
{
    public class GridModel : BaseGridModel<BaseGridObject>, IGridModel
    {
        private GridObjectType[,] _typeGrid;

        protected override void OnInitialize() { }

        public void Swap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (!IsInRange(sourceCoord) || !IsInRange(targetCoord)) return;

            var objA = GetGridObject(sourceCoord);
            var objB = GetGridObject(targetCoord);

            SetInternal(sourceCoord, objB);
            SetInternal(targetCoord, objA);
        }
        
        public GridObjectType[,] BuildGridTypeData()
        {
            if (_typeGrid == null || _typeGrid.GetLength(0) != Width || _typeGrid.GetLength(1) != Height)
                _typeGrid = new GridObjectType[Width, Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var obj = GetGridObject(new Vector2Int(x, y));
                    _typeGrid[x, y] = obj ? obj.ObjectType : default;
                }
            }

            return _typeGrid;
        }

        protected override void SetInternal(Vector2Int coord, BaseGridObject value, bool raiseEvent = true)
        {
            value?.SetCoordinate(coord);
            base.SetInternal(coord, value, raiseEvent);
        }
    }
}