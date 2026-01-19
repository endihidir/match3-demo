using System;
using Core.Handlers;
using Core.Item;
using Core.Models;
using Core.Services;
using Core.Utils;
using Core.Views;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
{
    public class GridPresenter : IInitializable, IDisposable
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
            _inputService.OnSwipe += OnInputGet;
        }

        private void OnInputGet(Vector2 sourcePos, Vector2Int inputDir)
        {
            var gridDir = _gridView.InputToGridDirection(inputDir);

            if (!TryPickInteractableCoord(sourcePos, out var sourceCoord)) return;

            _gridStateHandler.TryEnqueueInput(sourceCoord, gridDir);
        }

        private bool TryPickInteractableCoord(Vector2 screenPos, out Vector2Int coord)
        {
            coord = default;

            var worldPos = _gridView.Cam.ScreenToWorldPoint(screenPos);

            var hit = Physics2D.Raycast(worldPos, Vector2.zero, 0f, LayerMask.GetMask("GridItem"));

            if (!hit || !hit.transform.TryGetComponent(out BaseGridObject obj)) return false;

            if (obj.IsFallInProgress) return false;
            
            coord = obj.Coord;
            
            return true;
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
            _inputService.OnSwipe -= OnInputGet;
            _gridView.OnViewInitialized -= PlaceGridItems;
        }
    }
}