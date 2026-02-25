using System;
using Game.Grid.Handlers;
using Game.Grid.Services;
using Game.Level.Models;
using Game.Models;
using Game.Utils;
using Game.Views;
using UnityEngine;
using VContainer.Unity;

namespace Game.Presenters
{
    public sealed class GridPresenter : IInitializable, IDisposable
    {
        private readonly IGridModel _gridModel;
        private readonly ILevelGoalModel _goalModel;
        private readonly IGridView _gridView;
        private readonly IGridInputService _inputService;
        private readonly IGridStateHandler _stateHandler;

        public GridPresenter(IGridModel model, ILevelGoalModel goalModel, IGridView gridView, IGridInputService inputService, IGridStateHandler stateHandler)
        {
            _gridModel = model;
            _goalModel = goalModel;
            _gridView = gridView;
            _inputService = inputService;
            _stateHandler = stateHandler;
        }

        public void Initialize()
        {
            _gridView.OnViewInitialized += OnViewInitialized;
            _inputService.OnInputGet += OnInputGet;
            _goalModel.OnGoalCountUpdate += OnGoalCountUpdate;
            _goalModel.OnMoveCountUpdate += OnMoveCountUpdate;
        }
        
        private void OnViewInitialized()
        {
            PlaceGridItems();
            _stateHandler.Initialize();
            _inputService.Enable();
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

        private void OnInputGet(Vector2 mousePos, Vector2Int direction)
        {
            var gridDirection = _gridView.ToGridDirection(direction);
            var coord = _gridView.ScreenToGrid(mousePos);
            _stateHandler.TryEnqueueInput(coord, gridDirection);
        }
        
        private void OnGoalCountUpdate()
        {
            if (!_goalModel.IsAllGoalsComplete) return;
            _inputService.Disable();
        }

        private void OnMoveCountUpdate()
        {
            if (!_goalModel.IsAllMovesFinished) return;
            _inputService.Disable();
        }
        
        public void Dispose()
        {
            _inputService.OnInputGet -= OnInputGet;
            _gridView.OnViewInitialized -= OnViewInitialized;
            _goalModel.OnGoalCountUpdate -= OnGoalCountUpdate;
            _goalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}