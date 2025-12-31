using System;
using System.Buffers;
using Core.Utils;
using UnityEngine;

namespace Core.Models
{
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
            if (!IsInRange(coord.x, coord.y))
            {
                gridObject = null;
                return false;
            }

            gridObject = GridArray[coord.x, coord.y];
            return true;
        }

        public T GetGridObject(Vector2Int coord) => TryGetGridObject(coord, out var obj) ? obj : null;

        public void SetGridObject(Vector2Int coord, T value)
        {
            if (!IsInRange(coord.x, coord.y)) return;
            SetInternal(coord, value);
        }

        public bool TryGetNeighbour(Vector2Int sourceCoord, Vector2Int direction, out T neighbour)
        {
            neighbour = null;

            if (direction == Vector2Int.zero) return false;

            var targetCoord = sourceCoord + direction;
            if (!IsInRange(targetCoord.x, targetCoord.y)) return false;

            neighbour = GridArray[targetCoord.x, targetCoord.y];
            return neighbour != null;
        }

        public bool TryGetNeighbours(Vector2Int sourceCoord, out T[] neighbours)
        {
            if (!IsInRange(sourceCoord.x, sourceCoord.y))
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            var buffer = ArrayPool<T>.Shared.Rent(8);
            Array.Clear(buffer, 0, 8);

            try
            {
                if (!TryGetNeighboursNonAlloc(sourceCoord, buffer.AsSpan(0, 8), out var count) || count == 0)
                {
                    neighbours = Array.Empty<T>();
                    return false;
                }

                neighbours = new T[count];
                Array.Copy(buffer, neighbours, count);
                return true;
            }
            finally
            {
                ArrayPool<T>.Shared.Return(buffer, clearArray: true);
            }
        }

        public bool TryGetNeighboursNonAlloc(Vector2Int sourceCoord, Span<T> resultBuffer, out int count)
        {
            count = 0;

            if (!IsInRange(sourceCoord.x, sourceCoord.y)) return false;

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
            if (!IsInRange(coord.x, coord.y)) return false;
            return ActiveCells[coord.x, coord.y];
        }

        public bool IsInRange(Vector2Int coord) => IsInRange(coord.x, coord.y);

        public bool IsInRange(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        protected virtual void SetInternal(Vector2Int coord, T value, bool raiseEvent = true)
        {
            GridArray[coord.x, coord.y] = value;

            if (raiseEvent)
                OnUpdateCellData?.Invoke(value);
        }

        // "Fast" erişim gerekiyorsa sadece derived class kullanabilsin diye protected bıraktım.
        // Bounds garantisi sende olacak.
        protected T GetGridObjectUnsafe(int x, int y) => GridArray[x, y];
        protected bool IsCellActiveUnsafe(int x, int y) => ActiveCells[x, y];
    }
}