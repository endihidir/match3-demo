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
        private bool[,] _active;
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
            var stateData = new IGridItemState[width, height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var typeData = _levelDefinition.GridObjectTypes[y, x];
                    var gridPos  = new Vector2Int(x, y);

                    var item = _gridItemFactory.GetItem(typeData.gridItemKind, typeData.typeId, gridPos);
                    stateData[x, y] = item.State;
                }
            }

            var offset = 2f;
            
            _active = new bool[width, height];
        
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                _active[x, y] = true;
            
            _active[0, 0] = false;
            
            _active[0, 3] = false;
            _active[1, 3] = false;
            _active[2, 3] = false;
            
            _active[0, 4] = false;
            _active[1, 4] = false;
            _active[2, 4] = false;
            
            
            _model.Initialize(stateData);
           
            meshFilter.transform.localPosition = new Vector3(0, -offset, -0.3f);
            
            _model.BuildGridWithHoles(meshFilter,.15f,.1f,6,(x, y) => _active[x, y]);
            
            var bgTop = meshFilter.mesh.bounds.max.y;
            
            var originOffsetY = _model.GetTopY(_cam) - bgTop;
      
            _model.OriginOffset = new Vector3(0, originOffsetY + offset, 0);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var state = stateData[x, y];
                    if (state == null) continue;
                    
                    var worldPos = _model.GridToWorld(state.GridPos, _cam);
                    state.Transform.position = worldPos;
                    var size = new Vector2(_model.CellSize, _model.CellSize) * .75f;
                    state.SetSize(size);
                }
            }

            return this;
        }

        public void Refresh()
        {
            
        }
    }
}