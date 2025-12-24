using System;
using System.Collections.Generic;
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

        bool IsCellActive(Vector2Int pos);
        bool IsInRange(Vector2Int pos);

        bool TryGetNeighbour(Vector2Int pos, Vector2Int direction, out T neighbour);

        bool TryGetNeighbours(Vector2Int pos, out T[] neighbours);
        bool TryGetNeighboursNonAlloc(Vector2Int pos, Span<T> resultBuffer, out int count);
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

        public bool TryGetNeighbour(Vector2Int pos, Vector2Int direction, out T neighbour)
        {
            neighbour = null;

            if (direction == Vector2Int.zero) return false;

            var newPos = pos + direction;
            if (!IsInRange(newPos)) return false;

            neighbour = GetInternal(newPos);
            return neighbour != null;
        }

        public bool TryGetNeighbours(Vector2Int pos, out T[] neighbours)
        {
            if (!IsInRange(pos))
            {
                neighbours = Array.Empty<T>();
                return false;
            }

            var result = new List<T>(8);

            foreach (var direction in DirectionLookup.AllDirections)
            {
                if (!TryGetNeighbour(pos, direction, out var neighbour)) continue;
                result.Add(neighbour);
            }

            neighbours = result.ToArray();
            return neighbours.Length > 0;
        }

        public bool TryGetNeighboursNonAlloc(Vector2Int pos, Span<T> resultBuffer, out int count)
        {
            count = 0;

            if (!IsInRange(pos)) return false;

            foreach (var direction in DirectionLookup.AllDirections)
            {
                if (!TryGetNeighbour(pos, direction, out var neighbour)) continue;

                if (count >= resultBuffer.Length) return true;

                resultBuffer[count++] = neighbour;
            }

            return count > 0;
        }

        public bool IsCellActive(Vector2Int pos)
        {
            if (!IsInRange(pos)) return false;
            return ActiveCells[pos.x, pos.y];
        }

        public bool IsInRange(Vector2Int pos) => pos is { x: >= 0, y: >= 0 } && pos.x < Width && pos.y < Height;

        private T GetInternal(Vector2Int pos) => _gridArray[pos.x, pos.y];

        private void SetInternal(Vector2Int pos, T value, bool raiseEvent = true)
        {
            _gridArray[pos.x, pos.y] = value;

            if (raiseEvent)
                OnUpdateCellData?.Invoke(value);
        }
    }
}