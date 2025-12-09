using Core.Extensions;
using Core.Item;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.Utils;
using UnityEngine;

namespace Core.Presenters
{
    public interface IMatch3GridPresenter
    {
        IMatch3GridPresenter Initialize(IMatch3GridModel model, MeshFilter meshFilter, Transform pivot);
        void Refresh();
    }
    
    public class Match3GridPresenter : GridPresenter<IMatch3GridModel, IGridItemState>, IMatch3GridPresenter
    {
        private Camera _cam;
        private IMatch3GridModel _model;
        private readonly LevelDefinition _levelDefinition;
        private readonly IGridItemFactory _gridItemFactory;
        
        public Match3GridPresenter(ILevelDataService levelDataService, ILevelProgressReadModel progressReadModel, IGridItemFactory gridItemFactory)
        {
            _levelDefinition = levelDataService.LevelDefinitions[progressReadModel.CurrentLevelIndex];
            _gridItemFactory = gridItemFactory;
        }
        
        public IMatch3GridPresenter Initialize(IMatch3GridModel model, MeshFilter meshFilter, Transform pivot)
        {
            _cam = Camera.main;
            _model = model;
            _model.ScreenSidePaddingRatio = 5f;
            _model.Initialize(_levelDefinition.GridSize);
            
            var yOffset = pivot.position.y + (_model.Height * _model.CellSize * 0.5f);
            var originOffsetY = _model.GetTopY(_cam) - yOffset;
            _model.OriginOffset = new Vector3(0, originOffsetY, 0);
            
            var active = new bool[_model.Width, _model.Height];
            var cellSize = new Vector2(_model.CellSize, _model.CellSize);
            
            for (int i = 0; i < _model.Width * _model.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, _model.Width);
                
                var x = coordinate.x;
                var y = coordinate.y;
                
                var typeData = _levelDefinition.GridObjectTypes[x, y];
                active[x, y] = typeData.gridItemKind != GridItemKind.Regular || typeData.typeId != 0;
                if (!active[x, y]) continue;
                
                var item = _gridItemFactory.GetItem(typeData.gridItemKind, typeData.typeId, coordinate, cellSize);
                var itemState = item.State;
                var worldPos = _model.GridToWorld(coordinate, _cam);
                itemState.Transform.position = worldPos;
                _model.SetData(coordinate, itemState);
            }
            
            _model.BuildGridWithHoles(meshFilter,.25f,1f,20,(x, y) => active[x, y]);

            return this;
        }

        public void Refresh()
        {
            
        }
    }
}