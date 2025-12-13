using Core.Grid;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.Utils;
using Core.Views;
using UnityEngine;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Builder
{
    public interface IGridBuilder
    {
        IGridBuilder WithView(IGridView gridView);
        IGridBuilder WithGridSize(Vector2Int gridSize);
        IGridBuilder WithObjectTypes(GridObjectTypeData[,] itemObjectTypes);
        GridBuildResult Build();
    }
    
    public class GridBuilder : IGridBuilder
    {
        private readonly IGridModel _gridModel;
        private readonly IGridItemFactory _gridItemFactory;
        private readonly IGridLayoutCalculator _layoutCalculator;

        private Vector2Int _gridSize;
        private GridObjectTypeData[,] _gridObjectTypes;
        private IGridView _gridView;

        private GridLayout _gridLayout;

        public GridBuilder(IGridModel gridModel, IGridItemFactory gridItemFactory, IGridLayoutCalculator layoutCalculator)
        {
            _gridModel = gridModel;
            _gridItemFactory = gridItemFactory;
            _layoutCalculator = layoutCalculator;
        }

        public IGridBuilder WithView(IGridView gridView)
        {
            _gridView = gridView;
            return this;
        }

        public IGridBuilder WithGridSize(Vector2Int gridSize)
        {
            _gridSize = gridSize;
            return this;
        }

        public IGridBuilder WithObjectTypes(GridObjectTypeData[,] itemObjectTypes)
        {
            _gridObjectTypes = itemObjectTypes;
            return this;
        }

        public GridBuildResult Build()
        {
            if (_gridView == null)
            {
                EditorLogger.LogError("GridBuilder.Build failed: GridView is null! Call WithView() first.");
                ResetState();
                return default;
            }

            if (_gridSize.x <= 0 || _gridSize.y <= 0)
            {
                EditorLogger.LogError($"GridBuilder.Build failed: GridSize is invalid! ({_gridSize.x}, {_gridSize.y}) Call WithGridSize() with > 0 values.");
                ResetState();
                return default;
            }

            if (_gridObjectTypes == null)
            {
                EditorLogger.LogError("GridBuilder.Build failed: GridObjectTypes is null! Call WithObjectTypes() first.");
                ResetState();
                return default;
            }

            if (_gridObjectTypes.GetLength(0) != _gridSize.x || _gridObjectTypes.GetLength(1) != _gridSize.y)
            {
                EditorLogger.LogError($"GridBuilder.Build failed: GridObjectTypes size mismatch! Expected {_gridSize.x}x{_gridSize.y} but got {_gridObjectTypes.GetLength(0)}x{_gridObjectTypes.GetLength(1)}.");
                ResetState();
                return default;
            }

            Configure();
            CalculateOrigin();
            PlaceItems();
            GenerateMesh();
            ResetState();

            return new GridBuildResult(_gridModel, _gridLayout);
        }

        private void Configure()
        {
            var ls = _gridView.LayoutSettings;
            _gridModel.Initialize(_gridSize);
            _gridLayout = _layoutCalculator.CalculateLayout(_gridSize, _gridView.Cam, ls.ScreenSidePaddingRatio, ls.CellSpacingRatio, ls.MaxCellSize);

            _gridModel.BuildActiveCells(_gridObjectTypes);
        }

        private void CalculateOrigin()
        {
            var yOffset = _gridView.GridRoot.position.y + (_gridModel.Height * _gridLayout.CellSize * 0.5f);
            var topY = _layoutCalculator.GetTopY(_gridLayout, _gridView.Cam);
            var originOffsetY = topY - yOffset;
            _gridLayout.OriginOffset = new Vector3(0f, originOffsetY, 0f);
        }

        private void PlaceItems()
        {
            var cellSize = new Vector2(_gridLayout.CellSize, _gridLayout.CellSize);

            for (int i = 0; i < _gridModel.Width * _gridModel.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, _gridModel.Width);
                var x = coordinate.x;
                var y = coordinate.y;

                if (!_gridModel.ActiveData[x, y]) continue;

                var typeData = _gridObjectTypes[x, y];
                var item = _gridItemFactory.GetItem(typeData, coordinate, cellSize);
                item.Transform.position = _layoutCalculator.GridToWorld(_gridLayout, _gridSize, _gridView.Cam, coordinate);
                _gridModel.SetData(coordinate, item);
            }
        }

        private void GenerateMesh()
        {
            var ms = _gridView.MeshSettings;

            _gridModel.BuildGridMeshPipeFrame(_gridLayout, _gridView.GridMeshFilter, ms.FrameThickness, ms.CornerSmoothness,
                16, (x, y) => _gridModel.ActiveData[x, y]);
        }

        private void ResetState()
        {
            _gridView = null;
            _gridSize = default;
            _gridObjectTypes = null;
        }
        
    }
    public readonly struct GridBuildResult
    {
        public readonly IGridModel Model;
        public readonly GridLayout Layout;

        public GridBuildResult(IGridModel model, GridLayout layout)
        {
            Model = model;
            Layout = layout;
        }
    }
}