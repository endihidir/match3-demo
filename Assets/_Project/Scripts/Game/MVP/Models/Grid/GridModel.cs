using Core.Models;
using Game.Grid.Item;
using Game.Utils;
using UnityEngine;

namespace Game.Models
{
    public sealed class GridModel : BaseGridModel<BaseGridObject>, IGridModel
    {
        private GridObjectType[,] _typeGrid;
        private GridObjectType[] _typeGridArray;

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
        
        public GridObjectType[] BuildGridTypeDataArray()
        {
            if (_typeGridArray == null || _typeGridArray.Length != Width * Height)
                _typeGridArray = new GridObjectType[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var coord =  new Vector2Int(x, y);
                    var obj = GetGridObject(coord);
                    var index = GridIndexUtil.FromCoord(coord, Width);
                    _typeGridArray[index] = obj ? obj.ObjectType : default;
                }
            }

            return _typeGridArray;
        }

        protected override void SetInternal(Vector2Int coord, BaseGridObject value, bool raiseEvent = true)
        {
            value?.SetCoordinate(coord);
            base.SetInternal(coord, value, raiseEvent);
        }
    }
}