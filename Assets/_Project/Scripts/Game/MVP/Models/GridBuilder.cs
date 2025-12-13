using Core.Extensions;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.Utils;
using Core.Views;
using UnityEngine;

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

        private IGridView _gridView;
        private Vector2Int _gridSize;
        private GridObjectTypeData[,] _gridObjectTypes;

        public GridBuilder(IGridModel gridModel, IGridItemFactory gridItemFactory)
        {
            _gridModel = gridModel;
            _gridItemFactory = gridItemFactory;
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

            _gridModel.Initialize(_gridSize);
            _gridModel.BuildActiveCells(_gridObjectTypes);
            
            _gridView.Initialize(_gridSize, _gridModel.ActiveData);
            
            PlaceItems();
            ResetState();
            return new GridBuildResult(_gridModel, _gridView);
        }

        private void PlaceItems()
        {
            var cellSize = new Vector2(_gridView.Layout.CellSize, _gridView.Layout.CellSize);

            for (int i = 0; i < _gridModel.Width * _gridModel.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, _gridModel.Width);
                var x = coordinate.x;
                var y = coordinate.y;

                if (!_gridModel.ActiveData[x, y]) continue;

                var typeData = _gridObjectTypes[x, y];
                var item = _gridItemFactory.GetItem(typeData, coordinate, cellSize);
                item.Transform.position = _gridView.GridToWorld(_gridSize, coordinate);
                _gridModel.SetData(coordinate, item);
            }
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
        public readonly IGridView GridView;

        public GridBuildResult(IGridModel model, IGridView gridView)
        {
            Model = model;
            GridView = gridView;
        }
    }
}