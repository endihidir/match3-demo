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
        IMatch3GridPresenter Initialize(IMatch3GridModel model, MeshFilter meshFilter);
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
        
        public IMatch3GridPresenter Initialize(IMatch3GridModel model, MeshFilter meshFilter)
        {
            _cam = Camera.main;
            _model = model;
            _model.ScreenSidePaddingRatio = 5f;
            
            var width  = _levelDefinition.GridSize.x;
            var height = _levelDefinition.GridSize.y;
            
            _model.InitSize(width, height);
            
            var itemSize = new Vector2(_model.CellSize, _model.CellSize) * .75f;
            
            var active = new bool[width, height];

            for (int i = 0; i < height * width; i++)
            {
                var gridPos = CoordinateUtils.ToPos(i, width);
                var x = gridPos.x;
                var y = gridPos.y;
                var typeData = _levelDefinition.GridObjectTypes[x, y];
                active[x, y] = typeData.gridItemKind != GridItemKind.Regular || typeData.typeId != 0;
            }
            
            const float offset = 2f;
           
            meshFilter.transform.localPosition = new Vector3(0, -offset, -0.3f);
            
            _model.BuildGridWithHoles(meshFilter,.25f,1f,20,(x, y) => active[x, y]);
            
            var bgTop = meshFilter.mesh.bounds.max.y;
            
            var originOffsetY = _model.GetTopY(_cam) - bgTop;
      
            _model.OriginOffset = new Vector3(0, originOffsetY + offset, 0);
            
            for (int i = 0; i < height * width; i++)
            {
                var gridPos = CoordinateUtils.ToPos(i, width);
                
                var x = gridPos.x;
                var y = gridPos.y;
                
                if (!active[x, y]) continue;
                
                var typeData = _levelDefinition.GridObjectTypes[x, y];
                var item = _gridItemFactory.GetItem(typeData.gridItemKind, typeData.typeId, gridPos);
                var itemState = item.State;
                
                itemState.SetSize(itemSize);
                
                var worldPos = _model.GridToWorld(gridPos, _cam);
                itemState.Transform.position = worldPos;
                
                _model.SetData(gridPos, itemState);
            }

            return this;
        }

        public void Refresh()
        {
            
        }
    }
}