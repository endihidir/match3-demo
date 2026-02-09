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

        public HudPresenter(ILevelObjectiveModel levelObjectiveModel, IHudView hudView, IGoalFxHandler goalFxHandler, IGridStateHandler stateHandler)
        {
            _levelObjectiveModel = levelObjectiveModel;
            _hudView = hudView;
            _goalFxHandler = goalFxHandler;
            _gridStateHandler = stateHandler;
        }

        public void Initialize()
        {
            _levelObjectiveModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelObjectiveModel.OnGoalProgressUpdate += OnObjectiveCountUpdate;
            _gridStateHandler.Context.OnGridDestructionComplete += _goalFxHandler.PlayQueuedFX;
        }

        private void OnObjectiveCountUpdate(IDamageableGridObject damageableGridObject, Vector3 worldPos, Vector2 size)
        {
            if (damageableGridObject is not ObstacleObject obstacle) return;
           
            if (damageableGridObject.IsCollectible)
            {
                _goalFxHandler.QueueFX(obstacle.ObstacleType, worldPos, size);
            }
            else
            {
                _hudView.DecreaseGoalCount(obstacle.ObstacleType);
            }
        }

        private void OnMoveCountUpdate()
        {
            var moveCount = _levelObjectiveModel.MoveCount;
            
            _hudView.SetMoveCount(moveCount);
        }

        public void Dispose()
        {
            _gridStateHandler.Context.OnGridDestructionComplete -= _goalFxHandler.PlayQueuedFX;
            _levelObjectiveModel.OnGoalProgressUpdate -= OnObjectiveCountUpdate;
            _levelObjectiveModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}