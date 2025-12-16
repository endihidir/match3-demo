using System;
using Core.Models;
using Core.Utils;
using Core.Views;
using UnityEngine;
using IInitializable = VContainer.Unity.IInitializable;

namespace Core.Presenters
{
    public class GridPresenter : IInitializable, IDisposable
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;

        public GridPresenter(IGridModel model, IGridView gridView)
        {
            _gridModel = model;
            _gridView = gridView;
            _gridView.OnViewInitialized += OnViewInit;
        }
        
        private void OnViewInit() => PlaceGridItems();

        private void PlaceGridItems()
        {
            for (int i = 0; i < _gridModel.Width * _gridModel.Height; i++)
            {
                var coordinate = GridIndexUtil.ToCoord(i, _gridModel.Width);
                var item = _gridModel.GetGridObject(coordinate);
                if(!item) continue;
                
                var worldPos = _gridView.GridToWorld(coordinate);
                item.SetPosition(worldPos);
                item.SetCellSize(Vector2.one * _gridView.GetCellSize());
                item.SetParent(_gridView.GridObjectsParent);
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