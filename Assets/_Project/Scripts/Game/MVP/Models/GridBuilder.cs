using Core.Extensions;
using Core.Item.Factories;
using Core.Level;
using Core.Utils;
using Core.Views;
using UnityEngine;

namespace Core.Models
{
    public interface IGridBuilder
    {
        IGridBuilder WithView(IGridView gridView);
        IGridBuilder WithLevel(LevelDefinition levelDefinition);
        IGridBuilder Configure();
        IGridBuilder CalculateLayout();
        IGridBuilder PlaceItems();
        IGridBuilder GenerateMesh();
        IGridModel Build();
    }
    
     public class GridBuilder : IGridBuilder
    {
        private readonly IGridItemFactory _gridItemFactory;
        private LevelDefinition _levelDefinition;
        private readonly IGridModel _gridModel;
        
        private IGridView _gridView;

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

        public IGridBuilder WithLevel(LevelDefinition levelDefinition)
        {
            _levelDefinition = levelDefinition;
            return this;
        }

        public IGridBuilder Configure()
        {
            var viewSettings = _gridView.MeshSettings;
            
            _gridModel.Initialize(_levelDefinition.GridSize, viewSettings.ScreenSidePaddingRatio)
                  .CalculateCellSize(viewSettings.MaxCellSize);

            _gridModel.BuildActiveCells(_levelDefinition.GridObjectTypes);
            return this;
        }

        public IGridBuilder CalculateLayout()
        {
            var yOffset = _gridView.GridRoot.position.y + (_gridModel.Height * _gridModel.CellSize * 0.5f);
            var originOffsetY = _gridModel.GetTopY(_gridView.Cam) - yOffset;
            _gridModel.OriginOffset = new Vector3(0f, originOffsetY, 0f);
            return this;
        }

        public IGridBuilder PlaceItems()
        {
            var cellSize = new Vector2(_gridModel.CellSize, _gridModel.CellSize);

            for (int i = 0; i < _gridModel.Width * _gridModel.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, _gridModel.Width);
                var x = coordinate.x;
                var y = coordinate.y;

                if (!_gridModel.ActiveData[x, y]) continue;

                var typeData = _levelDefinition.GridObjectTypes[x, y];
                var item = _gridItemFactory.GetItem(typeData, coordinate, cellSize);

                item.Transform.position = _gridModel.GridToWorld(coordinate);
                _gridModel.SetData(coordinate, item);
            }

            return this;
        }

        public IGridBuilder GenerateMesh()
        {
            var viewSettings = _gridView.MeshSettings;

            _gridModel.BuildGridMeshPipeFrame(_gridView.GridMeshFilter, viewSettings.FrameThickness, viewSettings.CornerSmoothness, 16,
                (x, y) => _gridModel.ActiveData[x, y]);

            return this;
        }

        public IGridModel Build() => _gridModel;
    }
}