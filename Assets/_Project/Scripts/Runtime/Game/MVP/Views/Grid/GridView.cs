using System;
using Game.Configs;
using Core.Utils;
using Game.Extensions;
using NaughtyAttributes;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Game.Views
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
        [field: SerializeField] public GridMeshConfigSO MeshConfig { get; private set; }
        [field: SerializeField] public GridLayoutConfigSO LayoutConfig { get; private set; }
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
                screenSidePaddingRatio = LayoutConfig.ScreenSidePaddingRatio,
                cellSpacingRatio = LayoutConfig.CellSpacingRatio,
                originOffset = Vector3.zero,
                cellSize = 0f
            };
            
            var cellSize = layout.CalculateCellSize(_gridSize, Cam);
            layout.cellSize = Mathf.Clamp(cellSize, 0f, LayoutConfig.MaxCellSize);
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
        
        private void GenerateMesh()
        {
            var ms = MeshConfig;
            Layout.BuildGridMesh(_gridSize, GridMeshFilter, ms.FrameThickness, ms.CornerSmoothness, ms.GetCornerSegments(), IsCellActive);
        }
        
        public Vector2Int ToGridDirection(Vector2Int direction) => new(direction.x, -direction.y);
        
        public Vector2Int ScreenToGrid(Vector2 screenPos)
        {
            var worldPosition = Cam.ScreenToWorldPoint(screenPos);
            return WorldToGrid(worldPosition);
        }
        
        public Vector3 GridToScreen(Vector2Int coord)
        {
            var worldPos = GridToWorld(coord);
            return Cam.WorldToScreenPoint(worldPos);
        }
        
        public Vector3 GridToWorld(Vector2Int coord) => Layout.GridToWorld(_gridSize, coord, Cam);
        public Vector2Int WorldToGrid(Vector3 worldPos) => Layout.WorldToGrid(_gridSize, worldPos, Cam);
        public Vector2 SpriteToRectSize(Vector2 spriteSize) => UIWorldSpaceUtils.WorldSizeToUISize(spriteSize, Cam, Canvas);
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