using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.Models.Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Presenters
{
    public interface IGridPresenter
    {
        IGridPresenter Initialize(IGridModel model, IGridView view);
    }
    
    public class GridPresenter : IGridPresenter
    {
        private Camera _cam;
        private IGridModel _model;
        private readonly LevelDefinition _levelDefinition;
        private readonly IGridItemFactory _gridItemFactory;
        
        public GridPresenter(ILevelDataService levelDataService, ILevelProgressReadModel progressReadModel, IGridItemFactory gridItemFactory)
        {
            _levelDefinition = levelDataService.LevelDefinitions[progressReadModel.CurrentLevelIndex];
            _gridItemFactory = gridItemFactory;
        }
        
        public IGridPresenter Initialize(IGridModel model, IGridView view)
        {
            var builder = new GridBuilder(model, view, _levelDefinition, _gridItemFactory)
                        .WithScreenSidePadding(5f)
                        .WithMaxCellSize(3f)
                        .WithMeshSettings(0.25f, 1f, 8, 0.1f);

            builder.Build();
            
            return this;
        }
    }
}