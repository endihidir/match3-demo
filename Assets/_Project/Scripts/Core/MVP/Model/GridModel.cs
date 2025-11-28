using System;
using System.Collections.Generic;
using Core.Extensions;
using UnityEngine;

namespace Core.Models
{
    public interface IGridModel<T> where T : class
    {
        int Width { get; }
        int Height { get; }

        bool DrawGizmos { get; set; }
        float ScreenSidePaddingRatio { get; set; }
        float CellSpacingRatio { get; set; }
        Vector3 OriginOffset { get; set; }
        Color GizmosColor { get; set; }
        float CellSize { get; set; }
        bool UseHeightWidthBacking { get; }

        void Initialize(T[,] gridItemData, bool useHeightWidthBacking = true);
        T GetGridObject(Vector2Int gridPos);
        void SetGridObject(Vector2Int gridPos, T item);

        bool IsInRange(Vector2Int pos);

        bool TryGetNeighbor(Vector2Int pos, Direction2D direction2D, out T neighbour);
        bool TryGetNeighbors(Vector2Int pos, out T[] neighbours);
        bool TryGetNeighborsNonAlloc(Vector2Int pos, Span<T> resultBuffer, out int count);
    }
    
    public class GridModel<T> : IGridModel<T> where T : class
    {
        private int _width, _height;
        
        private T[,] _gridArray;

        public int Width => _width;
        public int Height => _height;

        public bool DrawGizmos { get; set; }
        public float ScreenSidePaddingRatio { get; set; }
        public float CellSpacingRatio { get; set; }
        public Vector3 OriginOffset { get; set; }
        public float CellSize { get; set; }
        public bool UseHeightWidthBacking { get; private set; }
        public Color GizmosColor { get; set; } = Color.yellow;

        private static readonly Direction2D[] DirectionList = (Direction2D[])Enum.GetValues(typeof(Direction2D));
        
        private static readonly Dictionary<Direction2D, Vector2Int> DirectionOffsets = new()
        {
            { Direction2D.Self,       new Vector2Int( 0,  0) },
            { Direction2D.Right,      new Vector2Int( 1,  0) },
            { Direction2D.Left,       new Vector2Int(-1,  0) },
            { Direction2D.Up,         new Vector2Int( 0, -1) },
            { Direction2D.Down,       new Vector2Int( 0,  1) },
            { Direction2D.RightUp,    new Vector2Int( 1, -1) },
            { Direction2D.LeftUp,     new Vector2Int(-1, -1) },
            { Direction2D.RightDown,  new Vector2Int( 1,  1) },
            { Direction2D.LeftDown,   new Vector2Int(-1,  1) }
        };

        public void Initialize(T[,] gridItemData, bool useHeightWidthBacking = true)
        {
            UseHeightWidthBacking = useHeightWidthBacking;
            _width = gridItemData.GetLength(0);
            _height = gridItemData.GetLength(1);
            
            _gridArray = UseHeightWidthBacking ? new T[_height, _width] : new T[_width, _height];
            
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    SetInternal(pos, gridItemData[x, y]);
                }
            }
            
            this.RecalculateCellSize();
        }

        private T GetInternal(Vector2Int pos)
        {
            return UseHeightWidthBacking ? _gridArray[pos.y, pos.x] : _gridArray[pos.x, pos.y];
        }

        private void SetInternal(Vector2Int pos, T value)
        {
            if (UseHeightWidthBacking)
            {
                _gridArray[pos.y, pos.x] = value;
            }
            else
            {
                _gridArray[pos.x, pos.y] = value;
            }
        }

        public T GetGridObject(Vector2Int pos)
        {
            if (!IsInRange(pos)) return null;
            
            return GetInternal(pos);
        }

        public void SetGridObject(Vector2Int pos, T value)
        {
            if (!IsInRange(pos)) return;
            
            SetInternal(pos, value);
        }

        public bool TryGetNeighbor(Vector2Int pos, Direction2D direction2D, out T neighbour)
        {
            neighbour = null;

            if (direction2D == Direction2D.None)
                return false;

            if (!DirectionOffsets.TryGetValue(direction2D, out var offset)) return false;

            var newPos = new Vector2Int(pos.x + offset.x, pos.y + offset.y);

            if (!IsInRange(newPos)) return false;

            neighbour = GetInternal(newPos);
            
            return neighbour != null;
        }

        public bool TryGetNeighbors(Vector2Int pos, out T[] neighbours)
        {
            if (!IsInRange(pos))
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            var result = new List<T>();

            foreach (var direction in DirectionList)
            {
                if (direction == Direction2D.None) continue;

                if (TryGetNeighbor(pos, direction, out var neighbour))
                {
                    result.Add(neighbour);
                }
            }

            neighbours = result.ToArray();
            return neighbours.Length > 0;
        }
        
        public bool TryGetNeighborsNonAlloc(Vector2Int pos, Span<T> resultBuffer, out int count)
        {
            count = 0;

            if (!IsInRange(pos)) return false;

            foreach (var direction in DirectionList)
            {
                if (direction == Direction2D.None) continue;
                
                if (!TryGetNeighbor(pos, direction, out var neighbour)) continue;
                
                if (neighbour == null) continue;

                if (count >= resultBuffer.Length) return true;

                resultBuffer[count++] = neighbour;
            }

            return count > 0;
        }
        
        public bool IsInRange(Vector2Int pos) => pos is { x: >= 0, y: >= 0 } && pos.x < Width && pos.y < Height;
    }
    
    public enum Direction2D
    {
        None = 0,
        Self = 1,
        Up = 2,
        Down = 3,
        Right = 4,
        Left = 5,
        LeftDown = 6,
        LeftUp = 7,
        RightUp = 8,
        RightDown = 9
    }
}