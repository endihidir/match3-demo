using System;
using UnityEngine;

namespace Game.Views
{
    public interface IGridView
    {
        event Action OnViewInitialized;
        Camera Cam { get; }
        bool IsInitialized { get; }
        Transform GridObjectsParent { get; }
        Transform FXParent { get; }
        void Initialize(int width, int height, bool[,] isCellActive);
        Vector2Int ScreenToGrid(Vector2 screenPos);
        Vector3 GridToWorld(Vector2Int coord);
        Vector2Int WorldToGrid(Vector3 worldPos);
        Vector3 GridToScreen(Vector2Int coord);
        Vector2 SpriteToRectSize(Vector2 spriteSize);
        Vector2Int InputToGridDirection(Vector2Int inputDirection);
        float GetCellSize();
    }
}