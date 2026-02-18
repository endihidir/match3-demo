using Core.Scene.Services;
using Game.View.Factories;
using Game.Grid.Handlers;
using Game.HUD.Handlers;
using Game.Level.Models;
using Game.Level.Services;
using Game.Models;
using Game.Views;
using VContainer;

namespace Game.Services
{
    public class GameplaySetupService : IGameplaySetupService
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        
        [Inject] private readonly IGridObjectHandler _gridObjectHandler;
        [Inject] private readonly IGridModel _gridModel;
        [Inject] private readonly IGridView _gridView;
        
        [Inject] private readonly IGoalSlotHandler _goalSlotHandler;
        [Inject] private readonly ILevelObjectiveModel _levelObjectiveModel;
        [Inject] private readonly IHudView _hudView;
        
        [Inject] private readonly ILevelEndView _levelEndView;
        [Inject] private readonly IFXViewFactory _fxViewFactory;
        
        public void SetupGameplay()
        {
            GridSetup();
            LevelEndSetup();
            HudSetup();
        }

        public void ResetGameplay()
        {
            ReleaseFactories();
            SetupGameplay();
        }
        
        public void ReleaseFactories()
        {
            _gridObjectHandler.ReleaseAllGridItems();
            _goalSlotHandler.ReleaseAllGoalSlots();
            _fxViewFactory.ReleaseFXesByType<BaseFxView>();
        }
        
        private void GridSetup()
        {
            var gridObjectTypes = _levelDefinitionProvider.GetGridObjectTypes();
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            _gridObjectHandler.PopulateGridItems(gridObjectTypes, width, height, out var gridItemObjects);
            _gridModel.Initialize(gridItemObjects, width, height, out var activeCells);
            _gridView.Initialize(width, height, activeCells);
        }
        
        private void LevelEndSetup() => _levelEndView.Initialize();
        
        private void HudSetup()
        {
            var levelGoals = _levelDefinitionProvider.GetLevelGoals();
            var levelMoveCount = _levelDefinitionProvider.GetMoveCount();
            _goalSlotHandler.PopulateSlotViews(levelGoals);
            _levelObjectiveModel.Initialize(levelGoals, levelMoveCount);
            _hudView.Initialize(levelGoals.Count, levelMoveCount);
        }
    }
}