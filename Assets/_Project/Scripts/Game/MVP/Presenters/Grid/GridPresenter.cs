using System;
using Core.Handlers;
using Core.Models;
using Core.Services;
using Core.Utils;
using Core.Views;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
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
            _goalModel.OnAllGoalsComplete += OnAllGoalsComplete;
            _goalModel.OnMoveCountUpdate += OnMoveCountUpdate;
        }
        
        private void OnViewInitialized()
        {
            PlaceGridItems();
            
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

        private void OnInputGet(Vector2 sourcePos, Vector2Int inputDir)
        {
            var gridDir = _gridView.InputToGridDirection(inputDir);
            var sourceCoord = _gridView.ScreenToGridCoordinate(sourcePos);
            _stateHandler.TryEnqueueInput(sourceCoord, gridDir);
        }
        
        private void OnAllGoalsComplete() => _inputService.Disable();
        
        private void OnMoveCountUpdate()
        {
            if (_goalModel.MoveCount > 0) return;
            
            _inputService.Disable();
        }
        
        public void Dispose()
        {
            _inputService.OnInputGet -= OnInputGet;
            _gridView.OnViewInitialized -= OnViewInitialized;
            _goalModel.OnAllGoalsComplete -= OnAllGoalsComplete;
            _goalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}