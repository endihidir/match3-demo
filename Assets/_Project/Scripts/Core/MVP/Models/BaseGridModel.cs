using System;
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
        T[,] GridArray { get; }

        event Action<T> OnGridObjectInitialized;
        event Action<T> OnUpdateCellData;
        event Action OnModelInitialized;

        IBaseGridModel<T> Initialize(T[,] value, int width, int height, out bool[,] activeCells);

        bool TryGetGridObject(Vector2Int coord, out T gridObject);
        T GetGridObject(Vector2Int coord);
        void SetGridObject(Vector2Int coord, T item);

        bool IsCellActive(Vector2Int coord);
        bool IsInRange(Vector2Int coord);
        bool IsInRange(int x, int y) => IsInRange(new Vector2Int(x, y));

        bool TryGetNeighbour(Vector2Int sourceCoord, Vector2Int direction, out T neighbour);

        bool TryGetNeighbours(Vector2Int sourceCoord, out T[] neighbours);
        bool TryGetNeighboursNonAlloc(Vector2Int sourceCoord, Span<T> resultBuffer, out int count);
    }

    public abstract class BaseGridModel<T> : IBaseGridModel<T> where T : class
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Vector2Int GridSize { get; private set; }
        public bool[,] ActiveCells { get; private set; }
        public T[,] GridArray { get; private set; }

        public event Action<T> OnGridObjectInitialized;
        public event Action<T> OnUpdateCellData;
        public event Action OnModelInitialized;

        public IBaseGridModel<T> Initialize(T[,] value, int width, int height, out bool[,] activeCells)
        {
            GridArray = new T[width, height];

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

                var gridObject = value[x, y];

                SetInternal(coordinate, gridObject, false);

                if (gridObject == null) continue;

                activeCells[x, y] = true;
                OnGridObjectInitialized?.Invoke(gridObject);
            }

            OnInitialize();
            OnModelInitialized?.Invoke();

            return this;
        }

        protected abstract void OnInitialize();

        public bool TryGetGridObject(Vector2Int coord, out T gridObject)
        {
            if (!IsInRange(coord))
            {
                gridObject = null;
                return false;
            }

            gridObject = GetInternal(coord);
            return true;
        }

        public T GetGridObject(Vector2Int coord)
        {
            if (!IsInRange(coord)) return null;
            return GetInternal(coord);
        }

        public void SetGridObject(Vector2Int coord, T value)
        {
            if (!IsInRange(coord)) return;
            SetInternal(coord, value);
        }

        public bool TryGetNeighbour(Vector2Int sourceCoord, Vector2Int direction, out T neighbour)
        {
            neighbour = null;

            if (direction == Vector2Int.zero) return false;

            var targetCoord = sourceCoord + direction;
            if (!IsInRange(targetCoord)) return false;

            neighbour = GetInternal(targetCoord);
            return neighbour != null;
        }

        public bool TryGetNeighbours(Vector2Int sourceCoord, out T[] neighbours)
        {
            if (!IsInRange(sourceCoord))
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            var buffer = new T[8];
            var span = buffer.AsSpan();

            if (!TryGetNeighboursNonAlloc(sourceCoord, span, out var count) || count == 0)
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            neighbours = new T[count];
            Array.Copy(buffer, neighbours, count);
            return true;
        }

        public bool TryGetNeighboursNonAlloc(Vector2Int sourceCoord, Span<T> resultBuffer, out int count)
        {
            count = 0;

            if (!IsInRange(sourceCoord)) return false;

            foreach (var direction in DirectionLookup.AllDirections)
            {
                if (!TryGetNeighbour(sourceCoord, direction, out var neighbour)) continue;

                if (count >= resultBuffer.Length) return true;

                resultBuffer[count++] = neighbour;
            }

            return count > 0;
        }

        public bool IsCellActive(Vector2Int coord)
        {
            if (!IsInRange(coord)) return false;
            return ActiveCells[coord.x, coord.y];
        }

        public bool IsInRange(Vector2Int coord) => coord is { x: >= 0, y: >= 0 } && coord.x < Width && coord.y < Height;

        protected T GetGridObjectFast(int x, int y) => GridArray[x, y];
        protected void SetGridObjectFast(int x, int y, T value, bool raiseEvent = true)
        {
            GridArray[x, y] = value;

            if (raiseEvent)
                OnUpdateCellData?.Invoke(value);
        }

        protected bool IsCellActiveFast(int x, int y) => ActiveCells[x, y];

        protected T GetInternal(Vector2Int coord) => GridArray[coord.x, coord.y];

        protected virtual void SetInternal(Vector2Int coord, T value, bool raiseEvent = true)
        {
            GridArray[coord.x, coord.y] = value;

            if (raiseEvent)
                OnUpdateCellData?.Invoke(value);
        }
    }
}
