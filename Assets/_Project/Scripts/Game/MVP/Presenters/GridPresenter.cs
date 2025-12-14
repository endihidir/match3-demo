using System;
using Core.Models;
using Core.Utils;
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
            _gridView.OnViewInitialized += OnViewInit;
        }
        
        private void OnViewInit()
        {
            for (int i = 0; i < _gridModel.Width * _gridModel.Height; i++)
            {
                var coordinate = CoordinateUtils.ToCoordinate(i, _gridModel.Width);
                var item = _gridModel.GetGridObject(coordinate);
                if(item == null) continue;
                var worldPos = _gridView.GridToWorld(_gridModel.GridSize, item.Coordinate);
                item.SetPosition(worldPos);
                item.SetCellSize(_gridView.Layout.cellSize);
            }
        }

        public void Initialize()
        {
            
        }
        
        public void Dispose()
        {
            _gridView.OnViewInitialized -= OnViewInit;
        }
    }
}