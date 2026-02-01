using System;
using Core.Extensions;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.SceneService;
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
        
        [Inject] private readonly IGridItemFactory _gridItemFactory;
        [Inject] private readonly IGridModel _gridModel;
        [Inject] private readonly IGridView _gridView;
        
        [Inject] private readonly IGoalSlotFactory _goalSlotFactory;
        [Inject] private readonly ILevelObjectiveModel _levelObjectiveModel;
        [Inject] private readonly IHudView _hudView;
        
        [Inject] private readonly ILevelEndView _levelEndView;
        
        public void Initialize() => _sceneLoadState.OnLoadComplete += OnSceneLoadComplete;

        private void OnSceneLoadComplete()
        {
            GridSetup();
            LevelEndSetup();
            HudSetup();
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
        
        private void LevelEndSetup() => _levelEndView.Initialize();
        
        private void HudSetup()
        {
            var levelGoals = _levelDefinitionProvider.GetLevelGoals();
            var levelMoveCount = _levelDefinitionProvider.GetMoveCount();
            _goalSlotFactory.PopulateSlotViews(levelGoals, out var slotViews);
            _levelObjectiveModel.Initialize(levelGoals, levelMoveCount);
            _hudView.Initialize(slotViews, levelMoveCount);
        }

        public void Dispose() => _sceneLoadState.OnLoadComplete -= OnSceneLoadComplete;
    }
}