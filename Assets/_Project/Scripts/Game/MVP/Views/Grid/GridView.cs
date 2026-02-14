using System;
using Core.Configs;
using Core.Extensions;
using Core.Utils;
using NaughtyAttributes;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Views
{
    public class GridView : MonoBehaviour, IGridView
    {
        [field: SerializeField, ReadOnly] public bool IsInitialized { get; private set; }
        [field: SerializeField, ReadOnly] public GridLayout Layout { get; private set; }
        [field: SerializeField] public Camera Cam { get; private set; }
        [field: SerializeField] public Canvas Canvas { get; private set; }
        [field: SerializeField] public Transform GridRoot { get; private set; }
        [field: SerializeField] public Transform GridObjectsParent { get; private set; }
        [field: SerializeField] public Transform FXParent { get; private set; }
        [field: SerializeField] public MeshFilter GridMeshFilter { get; private set; }
        [field: SerializeField] public GridMeshSettingsSO MeshSettings { get; private set; }
        [field: SerializeField] public GridLayoutSettingsSO LayoutSettings { get; private set; }
        [field: SerializeField] public bool DrawGridGizmos { get; private set; }
        [field: SerializeField, ShowIf(nameof(DrawGridGizmos))] public Color GizmosColor { get; private set; } = Color.yellow;
        public event Action OnViewInitialized;
        
        private Vector2Int _gridSize;
        private bool[,] _activeCells;
        
        public void Initialize(int width, int height, bool[,] isCellActive)
        {
            _gridSize = new Vector2Int(width, height);
            _activeCells = isCellActive;
            
            CalculateCellSize();
            CalculateOrigin();
            GenerateMesh();
            
            IsInitialized = true;
            OnViewInitialized?.Invoke();
        }

        private void CalculateCellSize()
        {
            var layout = new GridLayout
            {
                screenSidePaddingRatio = LayoutSettings.ScreenSidePaddingRatio,
                cellSpacingRatio = LayoutSettings.CellSpacingRatio,
                originOffset = Vector3.zero,
                cellSize = 0f
            };
            
            var cellSize = layout.CalculateCellSize(_gridSize, Cam);
            layout.cellSize = Mathf.Clamp(cellSize, 0f, LayoutSettings.MaxCellSize);
            Layout = layout;
        }
        
        private void CalculateOrigin()
        {
            var yOffset = GridRoot.position.y + (_gridSize.y * Layout.cellSize * 0.5f);
            var topY = Layout.GetTopYRaw(Cam);
            var originOffsetY = topY - yOffset;
            var layout = Layout;
            layout.originOffset = new Vector3(0f, originOffsetY, 0f);
            Layout = layout;
        }

        public Vector2Int ScreenToGridCoordinate(Vector2 mousePosition)
        {
            var worldPosition = Cam.ScreenToWorldPoint(mousePosition);
            var pos = WorldToGrid(worldPosition);
            return pos;
        }

        private void GenerateMesh()
        {
            var ms = MeshSettings;
            Layout.BuildGridMesh(_gridSize, GridMeshFilter, ms.FrameThickness, ms.CornerSmoothness, ms.GetCornerSegments(), IsCellActive);
        }
        public Vector3 GridToWorld(Vector2Int itemCoordinate) => Layout.GridToWorld(_gridSize, itemCoordinate, Cam);
        public Vector2Int WorldToGrid(Vector3 worldPosition) => Layout.WorldToGrid(_gridSize, worldPosition, Cam);
        public Vector3 GridToScreen(Vector2Int itemCoordinate)
        {
            var worldPos = Layout.GridToWorld(_gridSize, itemCoordinate, Cam);
            return Cam.WorldToScreenPoint(worldPos);
        }
        public Vector2 SpriteToUISize(Vector2 spriteSize) => UIWorldSpaceUtils.WorldSizeToUISize(spriteSize, Cam, Canvas);

        public Vector2Int InputToGridDirection(Vector2Int inputDirection)
        {
            if (inputDirection == Vector2Int.up) return Vector2Int.down;
            if (inputDirection == Vector2Int.down) return Vector2Int.up;
            return inputDirection;
        }
        public float GetCellSize() => Layout.cellSize;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(!IsInitialized) return;
            
            CalculateOrigin();

            if (!DrawGridGizmos) return;
            
            Layout.DrawGrid(_gridSize, GizmosColor, Cam, IsCellActive);
        }
#endif
        
        private bool IsCellActive(int x, int y) => _activeCells[x, y];
    }
}