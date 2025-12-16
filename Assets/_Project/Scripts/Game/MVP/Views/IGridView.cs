using System;
using UnityEngine;

namespace Core.Views
{
    public interface IGridView
    {
        event Action OnViewInitialized;
        Transform GridObjectsParent { get; }
        void Initialize(int width, int height, bool[,] isCellActive);
        Vector3 GridToWorld(Vector2Int itemCoordinate);
        Vector2Int WorldToGrid(Vector3 worldPosition);
        public float GetCellSize();
    }
}