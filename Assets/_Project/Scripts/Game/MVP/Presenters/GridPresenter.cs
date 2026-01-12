using System;
using Core.Handlers;
using Core.Models;
using Core.Services;
using Core.Utils;
using Core.Views;
using UnityEngine;

namespace Core.Presenters
{
    public class GridPresenter : IDisposable
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IInputService _inputService;
        private readonly IGridStateHandler _gridStateHandler;

        public GridPresenter(IGridModel model, IGridView gridView, IInputService inputService, IGridStateHandler gridStateHandler)
        {
            _gridModel = model;
            _gridView = gridView;
            _inputService = inputService;
            _gridStateHandler = gridStateHandler;
        }

        public void Initialize()
        {
            _gridView.OnViewInitialized += PlaceGridItems;
            _inputService.OnSwipe += OnSwipeGet;
        }

        private void OnSwipeGet(Vector2 screenPos, Vector2Int direction)
        {
            var sourceCoord = _gridView.ScreenToGridCoordinate(screenPos);

            var gridDirection = _gridView.InputDirectionToGridDirection(direction);

            _gridStateHandler.TryEnqueueInput(sourceCoord, gridDirection);
        }

        private void PlaceGridItems()
        {
            for (int i = 0; i < _gridModel.Width * _gridModel.Height; i++)
            {
                var coordinate = GridIndexUtil.ToCoord(i, _gridModel.Width);
                var item = _gridModel.GetGridObject(coordinate);
                if (!item) continue;

                var worldPos = _gridView.GridToWorld(coordinate);
                item.SetPosition(worldPos);
                item.SetSpriteSize(_gridView.GetCellSize());
                item.SetParent(_gridView.GridObjectsParent);
            }
        }

        public void Dispose()
        {
            _inputService.OnSwipe -= OnSwipeGet;
            _gridView.OnViewInitialized -= PlaceGridItems;
        }
    }
}