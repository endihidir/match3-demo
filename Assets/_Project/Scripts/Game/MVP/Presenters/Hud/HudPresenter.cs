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
        private readonly IGridStateHandler _gridStateHandler;

        public HudPresenter(ILevelObjectiveModel levelObjectiveModel, IHudView hudView, IGoalFxHandler goalFxHandler)
        {
            _levelObjectiveModel = levelObjectiveModel;
            _hudView = hudView;
            _goalFxHandler = goalFxHandler;
        }

        public void Initialize()
        {
            _levelObjectiveModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelObjectiveModel.OnGoalProgressUpdate += OnGoalProgressUpdate;
            _goalFxHandler.OnGoalFxComplete += UpdateGoalSlotView;
        }

        private void OnGoalProgressUpdate(IDamageableGridObject damageableGridObject, Vector3 startWorldPos, Vector2 cellSize)
        {
            if (damageableGridObject is not ObstacleObject obstacle) return;
       
            if (damageableGridObject.IsCollectible)
            {
                if (!_hudView.TryGetGoalSlotView(obstacle.ObstacleType, out var targetSlotView)) return;

                _goalFxHandler.PlayFX(targetSlotView, startWorldPos, cellSize, _hudView.GoalFxHolder);
            }
            else
            {
                UpdateGoalSlotView(obstacle.ObstacleType);
            }
        }

        private void UpdateGoalSlotView(ObstacleType obstacleType) => _hudView.DecreaseGoalCount(obstacleType);

        private void OnMoveCountUpdate()
        {
            var moveCount = _levelObjectiveModel.MoveCount;
            _hudView.SetMoveCount(moveCount);
        }

        public void Dispose()
        {
            _levelObjectiveModel.OnGoalProgressUpdate -= OnGoalProgressUpdate;
            _levelObjectiveModel.OnMoveCountUpdate -= OnMoveCountUpdate;
            _goalFxHandler.OnGoalFxComplete -= UpdateGoalSlotView;
        }
    }
}