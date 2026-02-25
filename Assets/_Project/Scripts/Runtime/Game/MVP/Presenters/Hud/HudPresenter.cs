using System;
using Game.Grid.Item;
using Game.Views;
using Game.HUD.Handlers;
using Game.HUD.Handlers.Data;
using Game.Level.Handlers;
using Game.Level.Models;
using VContainer.Unity;

namespace Game.Presenters
{
     public sealed class HudPresenter : IInitializable, IDisposable
    {
        private readonly ILevelGoalHandler _levelGoalHandler;
        private readonly ILevelGoalModel _levelGoalModel;
        private readonly IHudView _hudView;
        private readonly IGoalFxHandler _goalFxHandler;
        private readonly IGoalSlotHandler _goalSlotHandler;

        public HudPresenter(ILevelGoalModel levelGoalModel, IHudView hudView, ILevelGoalHandler levelGoalHandler, IGoalSlotHandler goalSlotHandler, IGoalFxHandler goalFxHandler)
        {
            _levelGoalModel = levelGoalModel;
            _hudView = hudView;
            _levelGoalHandler = levelGoalHandler;
            _goalSlotHandler = goalSlotHandler;
            _goalFxHandler = goalFxHandler;
        }

        public void Initialize()
        {
            _hudView.OnInitialize += OnHudViewInitialized;
            _levelGoalModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelGoalHandler.OnGoalCollected += OnGoalCollected;
            _goalFxHandler.OnGoalFxComplete += UpdateGoalSlotView;
        }

        private void OnHudViewInitialized() => PlaceSlotViews();
        
        private void PlaceSlotViews()
        {
            foreach (var goalSlotView in _goalSlotHandler.GoalSlotViews)
            {
                goalSlotView.transform.SetParent(_hudView.GoalsHolder, false);
            }
        }
        private void OnMoveCountUpdate() => _hudView.SetMoveCount(_levelGoalModel.MoveCount);

        private void OnGoalCollected(GoalCollectedData data)
        {
            var objectType = data.GridObjectType;
            
            if (data.GridObjectData.IsCollectible)
            {
                if (!_goalSlotHandler.TryGetGoalSlotView(objectType, out var targetSlotView)) return;
                
                _goalFxHandler.PlayFX(targetSlotView, data, _hudView.GoalFxHolder);
            }
            else
            {
                UpdateGoalSlotView(objectType);
            }
        }

        private void UpdateGoalSlotView(GridObjectType gridObjectType)
        {
            if(!_goalSlotHandler.TryGetGoalSlotView(gridObjectType, out var targetSlotView)) return;
            
            targetSlotView.DecrementGoalCount();
        }
        
        public void Dispose()
        {
            _hudView.OnInitialize -= OnHudViewInitialized;
            _levelGoalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
            _levelGoalHandler.OnGoalCollected -= OnGoalCollected;
            _goalFxHandler.OnGoalFxComplete -= UpdateGoalSlotView;
        }
    }
}