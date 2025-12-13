using System;
using Core.Models;
using Core.Views;

namespace Core.Presenters
{
    public interface IGridPresenter
    {
        void Initialize(IGridModel model, IGridView gridView);
    }

    public class GridPresenter : IGridPresenter, IDisposable
    {
        private IGridModel _gridModel;
        private IGridView _gridView;

        public void Initialize(IGridModel model, IGridView gridView)
        {
            _gridModel = model;
            _gridView = gridView;
            
        }

        public void Dispose()
        {
           
        }
    }
}
