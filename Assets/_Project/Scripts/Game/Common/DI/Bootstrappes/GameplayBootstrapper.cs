using System;
using Core.Extensions;
using Core.Generated;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.SceneService;
using Core.Services;
using Core.UI;
using Core.Views;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameplayBootstrapper : IInitializable, IDisposable
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        
        [Inject] private readonly IInputService _inputService;
        
        [Inject] private readonly IGridItemFactory _gridItemFactory;
        [Inject] private readonly IGridModel _gridModel;
        [Inject] private readonly IGridView _gridView;
        
        [Inject] private readonly IGoalSlotFactory _goalSlotFactory;
        [Inject] private readonly ILevelGoalModel _levelGoalModel;
        [Inject] private readonly IHudView _hudView;
        
        public void Initialize()
        {
            _sceneLoadState.OnLoadComplete += OnSceneLoadComplete;
            _levelGoalModel.OnAllGoalsComplete += OnAllGoalsComplete;
            _levelGoalModel.OnMoveCountUpdate += OnMoveCountUpdate;
        }

        private void OnSceneLoadComplete()
        {
            if(!_sceneLoadState.IsCurrentSceneEqualWidth(SceneGroupType.GameScene)) return;
            
            DisableInput();
            
            HudSetup();
            
            GridSetup();
            
            EnableInput();
        }

        private void HudSetup()
        {
            var levelGoals = _levelDefinitionProvider.GetLevelGoals();
            var levelMoveCount = _levelDefinitionProvider.GetMoveCount();
            _goalSlotFactory.PopulateSlotViews(levelGoals, out var slotViews);
            _levelGoalModel.Initialize(levelGoals, levelMoveCount);
            _hudView.Initialize(slotViews, levelMoveCount);
        }

        private void GridSetup()
        {
            var gridObjectTypes = _levelDefinitionProvider.GetGridObjectTypes();
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            _gridItemFactory.PopulateGridItems(gridObjectTypes, width, height, out var gridItemObjects);
            _gridModel.Initialize(gridItemObjects, width, height, out var activeCells);
            _gridView.Initialize(width, height, activeCells);
        }
        
        private void EnableInput() => _inputService.Enable();
        private void DisableInput() => _inputService.Disable();
        
        private void OnAllGoalsComplete() => DisableInput();
        private void OnMoveCountUpdate()
        {
            if (_levelGoalModel.MoveCount > 0) return;
            
            DisableInput();
        }
        
        public void Dispose()
        {
            _levelGoalModel.OnAllGoalsComplete -= OnAllGoalsComplete;
            _levelGoalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
            _sceneLoadState.OnLoadComplete -= OnSceneLoadComplete;
        }
    }
}