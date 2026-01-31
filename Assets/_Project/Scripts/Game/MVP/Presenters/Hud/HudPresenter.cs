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
        private readonly IGoalFxAnimationHandler _fxHandler;
        private readonly IGridStateHandler _gridStateHandler;

        public HudPresenter(ILevelObjectiveModel levelObjectiveModel, IHudView hudView, IGoalFxAnimationHandler fxHandler, IGridStateHandler stateHandler)
        {
            _levelObjectiveModel = levelObjectiveModel;
            _hudView = hudView;
            _fxHandler = fxHandler;
            _gridStateHandler = stateHandler;
        }

        public void Initialize()
        {
            _levelObjectiveModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelObjectiveModel.OnGoalProgressUpdate += OnObjectiveCountUpdate;
            _gridStateHandler.Context.OnGridDestructionComplete += _fxHandler.PlayQueuedAnimations;
        }

        private void OnObjectiveCountUpdate(IDamageableObstacle obstacle, Vector3 worldPos, Vector2 size)
        {
            if (obstacle.IsCollectible)
            {
                _fxHandler.QueueAnimation(obstacle, worldPos, size);
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
            _gridStateHandler.Context.OnGridDestructionComplete -= _fxHandler.PlayQueuedAnimations;
            _levelObjectiveModel.OnGoalProgressUpdate -= OnObjectiveCountUpdate;
            _levelObjectiveModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}