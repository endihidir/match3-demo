using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.Views;
using UnityEngine;

namespace Core.Presenters
{
    public interface IGridPresenter
    {
        void Initialize(IGridModel model, IGridView view);
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
        
        public void Initialize(IGridModel model, IGridView view)
        {
            var builder = new GridBuilder(model, view, _levelDefinition, _gridItemFactory).Build();
        }
    }
}