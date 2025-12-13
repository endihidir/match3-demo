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

        public GridPresenter(IGridModel model, IGameViewContext gameViewContext)
        {
            _gridModel = model;
            _gridView = gameViewContext.GridView;
        }
        
        public void Initialize()
        {
            
        }
        
        public void Dispose()
        {
           
        }

    }
}
