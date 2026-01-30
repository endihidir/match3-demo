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
        private readonly ILevelGoalModel _levelGoalModel;
        private readonly IHudView _hudView;
        private readonly IGoalFxAnimationHandler _fxHandler;
        private readonly IGridStateHandler _gridStateHandler;

        public HudPresenter(ILevelGoalModel levelGoalModel, IHudView hudView, IGoalFxAnimationHandler fxHandler, IGridStateHandler stateHandler)
        {
            _levelGoalModel = levelGoalModel;
            _hudView = hudView;
            _fxHandler = fxHandler;
            _gridStateHandler = stateHandler;
        }

        public void Initialize()
        {
            _levelGoalModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelGoalModel.OnGoalCountUpdate += OnGoalCountUpdate;
            _gridStateHandler.Context.OnGridDestructionComplete += _fxHandler.PlayQueuedAnimations;
        }

        private void OnGoalCountUpdate(IDamageableObstacle obstacle, Vector3 worldPos, Vector2 size)
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
            var moveCount = _levelGoalModel.MoveCount;
            
            _hudView.SetMoveCount(moveCount);
        }

        public void Dispose()
        {
            _gridStateHandler.Context.OnGridDestructionComplete -= _fxHandler.PlayQueuedAnimations;
            _levelGoalModel.OnGoalCountUpdate -= OnGoalCountUpdate;
            _levelGoalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}