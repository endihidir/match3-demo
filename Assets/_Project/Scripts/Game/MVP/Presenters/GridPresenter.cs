using System;
using Core.Models;
using Core.Views;
using VContainer.Unity;

namespace Core.Presenters
{
    public interface IGridPresenter
    {
        
    }

    public class GridPresenter : IGridPresenter, IInitializable, IDisposable
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;

        public GridPresenter(IGridModel model, IGridView gridView)
        {
            _gridModel = model;
            _gridView = gridView;
        }
        
        public void Initialize()
        {
            
        }
        
        public void Dispose()
        {
           
        }

    }
}
