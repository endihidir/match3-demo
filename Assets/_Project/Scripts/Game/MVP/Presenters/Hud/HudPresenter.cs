using System;
using Core.Handlers;
using Core.Item;
using Core.Models;
using Core.UI;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
{
     public sealed class HudPresenter : IInitializable, IDisposable
    {
        private readonly ILevelObjectiveModel _levelObjectiveModel;
        private readonly IHudView _hudView;
        private readonly IGoalFxHandler _goalFxHandler;
        private readonly IGoalSlotHandler _goalSlotHandler;

        public HudPresenter(ILevelObjectiveModel levelObjectiveModel, IHudView hudView, IGoalSlotHandler goalSlotHandler, IGoalFxHandler goalFxHandler)
        {
            _levelObjectiveModel = levelObjectiveModel;
            _hudView = hudView;
            _goalSlotHandler = goalSlotHandler;
            _goalFxHandler = goalFxHandler;
        }

        public void Initialize()
        {
            _hudView.OnInitialize += OnHudViewInitialized;
            _levelObjectiveModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelObjectiveModel.OnGoalProgressUpdate += OnGoalProgressUpdate;
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
        private void OnMoveCountUpdate() => _hudView.SetMoveCount(_levelObjectiveModel.MoveCount);

        private void OnGoalProgressUpdate(IDamageableGridObject damageableObj, Vector3 startWorldPos, Vector2 cellSize)
        {
            if (damageableObj.IsCollectible)
            {
                if (!_goalSlotHandler.TryGetGoalSlotView(damageableObj.ObstacleType, out var targetSlotView)) return;

                _goalFxHandler.PlayFX(targetSlotView, startWorldPos, cellSize, _hudView.GoalFxHolder);
            }
            else
            {
                UpdateGoalSlotView(damageableObj.ObstacleType);
            }
        }

        private void UpdateGoalSlotView(ObstacleType obstacleType)
        {
            if(!_goalSlotHandler.TryGetGoalSlotView(obstacleType, out var targetSlotView)) return;
            
            targetSlotView.DecrementGoalCount();
        }
        
        public void Dispose()
        {
            _hudView.OnInitialize -= OnHudViewInitialized;
            _levelObjectiveModel.OnMoveCountUpdate -= OnMoveCountUpdate;
            _levelObjectiveModel.OnGoalProgressUpdate -= OnGoalProgressUpdate;
            _goalFxHandler.OnGoalFxComplete -= UpdateGoalSlotView;
        }
    }
}