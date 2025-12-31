using System;
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
        T GetGridObjectFast(int x, int y);
        void SetGridObject(Vector2Int coord, T item);

        bool IsCellActive(Vector2Int coord);
        bool IsCellActiveFast(int x, int y);
        bool IsInRange(Vector2Int coord);
        bool IsInRange(int x, int y) => IsInRange(new Vector2Int(x, y));

        bool TryGetNeighbour(Vector2Int sourceCoord, Vector2Int direction, out T neighbour);

        bool TryGetNeighbours(Vector2Int sourceCoord, out T[] neighbours);
        bool TryGetNeighboursNonAlloc(Vector2Int sourceCoord, Span<T> resultBuffer, out int count);
    }
}