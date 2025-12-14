using System;
using Core.Config;
using Core.Extensions;
using NaughtyAttributes;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Views
{
    public interface IGridView
    { 
        GridLayout Layout { get; }
        void Initialize(int width, int height, bool[,] isCellActive);
        Vector3 GridToWorld(Vector2Int size, Vector2Int itemCoordinate);
        event Action OnViewInitialized;
    }
    
    public class GridView : MonoBehaviour, IGridView
    {
        [field: SerializeField] private Camera Cam { get; set; }
        [field: SerializeField] private Transform GridRoot { get; set; }
        [field: SerializeField] private MeshFilter GridMeshFilter { get; set; }
        [field: SerializeField] private GridMeshSettingsConfig MeshSettings { get; set; }
        [field: SerializeField] private GridLayoutSettingsConfig LayoutSettings { get; set; }
        [field: SerializeField, ReadOnly] public GridLayout Layout { get; private set; }
        public event Action OnViewInitialized;

        public void Initialize(int width, int height, bool[,] isCellActive)
        {
            var gridSize = new Vector2Int(width, height);
            CalculateCellSize(gridSize);
            CalculateOrigin(gridSize.y);
            GenerateMesh(gridSize, isCellActive);
            OnViewInitialized?.Invoke();
        }
        
        private void CalculateCellSize(Vector2Int gridSize)
        {
            if (!Cam) return;
            
            var layout = new GridLayout
            {
                screenSidePaddingRatio = LayoutSettings.ScreenSidePaddingRatio,
                cellSpacingRatio = LayoutSettings.CellSpacingRatio,
                originOffset = Vector3.zero,
                cellSize = 0f
            };
            
            var cellSize = layout.CalculateCellSize(gridSize, Cam);
            layout.cellSize = Mathf.Clamp(cellSize, 0f, LayoutSettings.MaxCellSize);
            Layout = layout;
        }
        
        private void CalculateOrigin(int gridHeight)
        {
            var yOffset = GridRoot.position.y + (gridHeight * Layout.cellSize * 0.5f);
            var topY = Layout.GetTopY(Cam);
            var originOffsetY = topY - yOffset;
            var layout = Layout;
            layout.originOffset = new Vector3(0f, originOffsetY, 0f);
            Layout = layout;
        }

        private void GenerateMesh(Vector2Int size, bool[,] isCellActive)
        {
            var ms = MeshSettings;
            Layout.BuildGridMesh(size, GridMeshFilter, ms.FrameThickness, ms.CornerSmoothness,16, CellActive);
            return;
            bool CellActive(int x, int y) => isCellActive[x, y];
        }
        
        public Vector3 GridToWorld(Vector2Int size, Vector2Int itemCoordinate) => Layout.GridToWorld(size, itemCoordinate, Cam);
    }
}