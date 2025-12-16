using System;
using Core.Models;
using Core.Services;
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
        private readonly IInputService _inputService;

        public GridPresenter(IGridModel model, IGridView gridView, IInputService inputService)
        {
            _gridModel = model;
            _gridView = gridView;
            _inputService = inputService;
            _inputService.OnInput += OnInputGet;
            _gridView.OnViewInitialized += OnViewInit;
        }
        
        public void Initialize()
        {
        }

        private void OnInputGet(Vector2 position, Direction2D direction2D)
        {
            var gridCoord = _gridView.GetMouseToGridPos(position);

            if (_gridModel.TryGetGridObject(gridCoord, out var gridItem))
            {
                EditorLogger.LogError($"{gridItem} - {direction2D}");
            }
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
        
        public void Dispose()
        {
            _inputService.OnInput -= OnInputGet;
            _gridView.OnViewInitialized -= OnViewInit;
        }
    }
}