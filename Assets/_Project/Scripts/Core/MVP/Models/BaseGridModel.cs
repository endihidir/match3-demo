using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using UnityEngine;

namespace Core.Models
{
    public interface IBaseGridModel<T> where T : class
    {
        int Width { get; }
        int Height { get; }
        Vector2Int GridSize { get; }
        bool[,] ActiveCells { get; }
        event Action<T> OnGridObjectInitialized;
        event Action<T> OnUpdateCellData;
        event Action OnModelInitialized;
        
        IBaseGridModel<T> Initialize(T[,] value, int width, int height, out bool[,] activeCells);
        bool TryGetGridObject(Vector2Int pos, out T gridObject);
        T GetGridObject(Vector2Int gridPos);
        void SetGridObject(Vector2Int gridPos, T item);

        bool IsInRange(Vector2Int pos);
        bool TryGetNeighbour(Vector2Int pos, Vector2Int direction, out T neighbour);
        bool TryGetNeighbor(Vector2Int pos, Direction2D direction2D, out T neighbour);
        bool TryGetNeighbors(Vector2Int pos, out T[] neighbours);
        bool TryGetNeighborsNonAlloc(Vector2Int pos, Span<T> resultBuffer, out int count);
    }
    
    public abstract class BaseGridModel<T> : IBaseGridModel<T> where T : class
    {
        private T[,] _gridArray;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2Int GridSize { get; private set; }
        public bool[,] ActiveCells { get; private set; }

        public event Action<T> OnGridObjectInitialized;
        public event Action<T> OnUpdateCellData;
        public event Action OnModelInitialized;

        private static readonly Direction2D[] DirectionList = (Direction2D[])Enum.GetValues(typeof(Direction2D));
        
        private static readonly Dictionary<Direction2D, Vector2Int> DirectionOffsets = new()
        {
            { Direction2D.Self, new Vector2Int(0, 0) },
            { Direction2D.Right, new Vector2Int(1, 0) },
            { Direction2D.Left, new Vector2Int(-1, 0) },
            { Direction2D.Up, new Vector2Int(0, -1) },
            { Direction2D.Down, new Vector2Int(0, 1) },
            { Direction2D.RightUp, new Vector2Int(1, -1) },
            { Direction2D.LeftUp, new Vector2Int(-1, -1) },
            { Direction2D.RightDown, new Vector2Int(1, 1) },
            { Direction2D.LeftDown, new Vector2Int(-1, 1) }
        };
        
        private static readonly List<Vector2Int> Offsets = new()
        {
            { new Vector2Int(0, 0) },
            { new Vector2Int(1, 0) },
            { new Vector2Int(-1, 0) },
            { new Vector2Int(0, -1) },
            { new Vector2Int(0, 1) },
            { new Vector2Int(1, -1) },
            { new Vector2Int(-1, -1) },
            { new Vector2Int(1, 1) },
            { new Vector2Int(-1, 1) }
        };
        
        public IBaseGridModel<T> Initialize(T[,] value, int width, int height, out bool[,] activeCells)
        {
            _gridArray = new T[width, height];
            Width = width;
            Height = height;
            GridSize = new Vector2Int(width, height);
            activeCells = new bool[width, height];
            ActiveCells = activeCells;
            
            for (int i = 0; i < Width * Height; i++)
            {
                var coordinate = GridIndexUtil.ToCoord(i, Width);
                var x = coordinate.x;
                var y = coordinate.y;
                
                SetInternal(coordinate, value[x, y], false);
                var gridObject = GetInternal(coordinate);
                if (gridObject == null) continue;
                
                activeCells[x, y] = true;
                OnGridObjectInitialized?.Invoke(gridObject);
            }
            
            OnInitialize();
            OnModelInitialized?.Invoke();
            return this;
        }
        protected abstract void OnInitialize();

        public bool TryGetGridObject(Vector2Int pos, out T gridObject)
        {
            if (!IsInRange(pos))
            {
                gridObject = null;
                return false;
            }
            
            gridObject = GetInternal(pos);
            return true;
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
        
        public bool TryGetNeighbour(Vector2Int pos, Vector2Int direction, out T neighbour)
        {
            neighbour = null;

            if (direction == Vector2Int.zero) return false;

            var newPos = pos + direction;

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
                if (TryGetNeighbor(pos, direction, out var neighbour))
                {
                    result.Add(neighbour);
                }
            }

            neighbours = result.ToArray();
            return neighbours.Length > 0;
        }
        
        public bool TryGetNeighbors(Vector2Int pos, int range, out T[] neighbours, IReadOnlyCollection<Vector2Int> ignoredDirections = null)
        {
            if (!IsInRange(pos) || range <= 0)
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            var result = new List<T>();

            foreach (var direction in Offsets)
            {
                if (ignoredDirections != null && ignoredDirections.Contains(direction))
                    continue;

                var currentPos = pos;

                for (int i = 0; i < range; i++)
                {
                    if (!TryGetNeighbour(currentPos, direction, out var neighbour))
                        break;

                    result.Add(neighbour);
                    currentPos += direction;
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
        private T GetInternal(Vector2Int pos) => _gridArray[pos.x, pos.y];
        private void SetInternal(Vector2Int pos, T value, bool raiseEvent = true)
        {
            _gridArray[pos.x, pos.y] = value;
            if (raiseEvent) OnUpdateCellData?.Invoke(value);
        }
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