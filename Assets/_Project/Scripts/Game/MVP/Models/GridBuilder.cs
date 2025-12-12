using Core.Extensions;
using Core.Item.Factories;
using Core.Level;
using Core.Utils;
using Core.Views;
using UnityEngine;

namespace Core.Models
{
    public class GridBuilder
    {
        private readonly IGridModel _model;
        private readonly IGridView _gridView;
        private readonly LevelDefinition _levelDefinition;
        private readonly IGridItemFactory _gridItemFactory;

        public GridBuilder(IGridModel model, IGridView gridView, LevelDefinition levelDefinition, IGridItemFactory gridItemFactory)
        {
            _model = model;
            _gridView = gridView;
            _levelDefinition = levelDefinition;
            _gridItemFactory = gridItemFactory;
        }

        public IGridModel Build()
        {
            var settings = _gridView.MeshSettings;
            _model.ScreenSidePaddingRatio = settings.ScreenSidePaddingRatio;
            _model.Initialize(_levelDefinition.GridSize, maxCellSize: settings.MaxCellSize);
            _model.FillActiveStatus(_levelDefinition.GridObjectTypes);
            
            var yOffset = _gridView.GridRoot.position.y + (_model.Height * _model.CellSize * 0.5f);
            var originOffsetY = _model.GetTopY(_gridView.Cam) - yOffset;
            _model.OriginOffset = new Vector3(0, originOffsetY, 0);

            var cellSize = new Vector2(_model.CellSize, _model.CellSize);

            for (int i = 0; i < _model.Width * _model.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, _model.Width);
                var x = coordinate.x;
                var y = coordinate.y;

                if (!_model.ActiveData[x, y]) continue;

                var typeData = _levelDefinition.GridObjectTypes[x, y];
                var item = _gridItemFactory.GetItem(typeData, coordinate, cellSize);
                var worldPos = _model.GridToWorld(coordinate);
                item.Transform.position = worldPos;
                _model.SetData(coordinate, item);
            }
            
            _model.BuildGridMeshPipeFrame(_gridView.GridMeshFilter, settings.FrameThickness, settings.CornerSmoothness, 
                                          16, (x, y) => _model.ActiveData[x, y]);
            return _model;
        }
    }
}