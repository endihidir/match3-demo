using System;
using Core.Handlers;
using Core.Item;
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
            var objCoordinate = _gridView.ScreenToGridCoordinate(screenPos);

            if (!_gridModel.TryGetGridObject(objCoordinate, out var gridItemObject)) return;
            
            if (!gridItemObject) return;

            ProcessSwipe(gridItemObject, objCoordinate, direction);
        }
        
        private void ProcessSwipe(BaseItemObject obj, Vector2Int coordinate, Vector2Int direction)
        {
            if (!IsInteractable(obj)) return;
            
            if (!CanSwap(obj, coordinate, direction, out var targetCoord)) return;

            _gridStateHandler.TryEnqueueSwap(coordinate, targetCoord);
        }

        private static bool IsInteractable(BaseItemObject obj)
        {
            if (!obj) return false;
            if (obj.IsEmpty) return false;
            return !obj.IsShiftInProgress;
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
        
        private bool CanSwap(BaseItemObject obj, Vector2Int sourceCoord, Vector2Int direction, out Vector2Int targetCoord)
        {
            targetCoord = default;

            var isNotBoosterAndTap = obj is not BoosterObject && direction == Vector2Int.zero;

            if (isNotBoosterAndTap || obj.IsStationary || obj is ObstacleObject || 
                !_gridModel.TryGetNeighbour(sourceCoord, direction, out var neighbour) 
                || !neighbour || neighbour.IsStationary || neighbour is ObstacleObject)
            {
                obj.ItemAnimation.Shake();
                return false;
            }

            if (!IsInteractable(neighbour)) return false;
            
            targetCoord = sourceCoord + direction;
            
            return true;
        }

        public void Dispose()
        {
            _inputService.OnSwipe -= OnSwipeGet;
            _gridView.OnViewInitialized -= PlaceGridItems;
        }
    }
}