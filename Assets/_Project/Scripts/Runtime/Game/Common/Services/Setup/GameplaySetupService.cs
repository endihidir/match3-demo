using Core.Scene.Services;
using Game.View.Factories;
using Game.Grid.Handlers;
using Game.Grid.Item;
using Game.Grid.Item.Factories;
using Game.HUD.Handlers;
using Game.Level.Handlers;
using Game.Level.Models;
using Game.Level.Services;
using Game.Models;
using Game.Views;
using VContainer;

namespace Game.Services
{
    public sealed class GameplaySetupService : IGameplaySetupService
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        
        [Inject] private readonly IGridObjectCreateHandler _gridObjectCreateHandler;
        [Inject] private readonly IGridModel _gridModel;
        [Inject] private readonly IGridView _gridView;
        
        [Inject] private readonly IGoalSlotHandler _goalSlotHandler;
        [Inject] private readonly ILevelGoalModel _levelGoalModel;
        [Inject] private readonly IHudView _hudView;
        
        [Inject] private readonly ILevelResultHandler _levelResultHandler;
        [Inject] private readonly ILevelEndView _levelEndView;
        
        [Inject] private readonly IGridObjectFactory _gridObjectFactory;
        [Inject] private readonly ISlotViewFactory _slotViewFactory;
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
            _gridObjectFactory.ReleaseObjectsByType<BaseGridObject>();
            _slotViewFactory.ReleaseSlotsByType<BaseSlotView>();
            _fxViewFactory.ReleaseFXesByType<BaseFxView>();
        }
        
        private void GridSetup()
        {
            var gridObjectTypes = _levelDefinitionProvider.GetGridObjectTypes();
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            _gridObjectCreateHandler.PopulateGrid(gridObjectTypes, width, height, out var gridItemObjects);
            _gridModel.Initialize(gridItemObjects, width, height, out var activeCells);
            _gridView.Initialize(width, height, activeCells);
        }
        
        private void LevelEndSetup()
        {
            _levelResultHandler.Initialize();
            _levelEndView.Initialize();
        }

        private void HudSetup()
        {
            var levelGoals = _levelDefinitionProvider.GetLevelGoals();
            var levelMoveCount = _levelDefinitionProvider.GetMoveCount();
            _goalSlotHandler.PopulateSlotViews(levelGoals);
            _levelGoalModel.Initialize(levelGoals, levelMoveCount);
            _hudView.Initialize(levelGoals.Count, levelMoveCount);
        }
    }
}