using System;
using UnityEngine;

namespace Core.Views
{
    public interface IGridView
    {
        event Action OnViewInitialized;
        Camera Cam { get; }
        bool IsInitialized { get; }
        Transform GridObjectsParent { get; }
        Transform FXParent { get; }
        void Initialize(int width, int height, bool[,] isCellActive);
        Vector2Int ScreenToGridCoordinate(Vector2 mousePosition);
        Vector3 GridToWorld(Vector2Int itemCoordinate);
        Vector2Int WorldToGrid(Vector3 worldPosition);
        Vector3 GridToScreen(Vector2Int itemCoordinate);
        Vector2 SpriteToUISize(Vector2 spriteSize);
        Vector2Int InputToGridDirection(Vector2Int inputDirection);
        float GetCellSize();
    }
}