using System;
using Core.Level;
using Core.Models;
using Core.Views;

namespace Core.Presenters
{
    public interface IGridPresenter
    {
        void Initialize(IGridView gridView, LevelDefinition levelDefinition);
    }
    
    public class GridPresenter : IGridPresenter, IDisposable
    {
        private readonly IGridBuilder _gridBuilder;
        private IGridModel _gridModel;
        public GridPresenter(IGridBuilder gridBuilder) => _gridBuilder = gridBuilder;

        public void Initialize(IGridView gridView, LevelDefinition levelDefinition)
        {
            _gridModel = _gridBuilder.WithView(gridView)
                                     .WithLevel(levelDefinition)
                                     .Configure()
                                     .CalculateLayout()
                                     .PlaceItems()
                                     .GenerateMesh()
                                     .Build();
        }

        public void Dispose()
        {
            
        }
    }
}