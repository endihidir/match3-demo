using System;
using Core.Models;
using GridLayout = Core.Grid.GridLayout;

namespace Core.Presenters
{
    public interface IGridPresenter
    {
        void Initialize(IGridModel model, GridLayout layout);
    }

    public class GridPresenter : IGridPresenter, IDisposable
    {
        private IGridModel _gridModel;
        private GridLayout _layout;

        public void Initialize(IGridModel model, GridLayout layout)
        {
            _gridModel = model;
            _layout = layout;
            
        }

        public void Dispose()
        {
           
        }
    }
}
