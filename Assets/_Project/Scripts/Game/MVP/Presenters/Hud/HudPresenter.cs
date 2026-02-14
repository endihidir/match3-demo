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

        private void OnGoalProgressUpdate(IDamageableGridObject damageableObj, Vector3 startWorldPos, Vector2 cellSize)
        {
            if (damageableObj.IsCollectible)
            {
                if (!_hudView.TryGetGoalSlotView(damageableObj.ObstacleType, out var targetSlotView)) return;

                _goalFxHandler.PlayFX(targetSlotView, startWorldPos, cellSize, _hudView.GoalFxHolder);
            }
            else
            {
                UpdateGoalSlotView(damageableObj.ObstacleType);
            }
        }

        private void UpdateGoalSlotView(ObstacleType obstacleType)
        {
            if(!_hudView.TryGetGoalSlotView(obstacleType, out var targetSlotView)) return;
            
            if(!_levelObjectiveModel.TryGetGoal(obstacleType, out var goal)) return;
            
            targetSlotView.AnimateToCount(goal.Count);
        }

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