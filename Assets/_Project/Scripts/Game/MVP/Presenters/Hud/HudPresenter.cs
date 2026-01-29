using System;
using System.Collections.Generic;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
using Core.UI;
using Core.Utils;
using DG.Tweening;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
{
    public sealed class HudPresenter : IInitializable, IDisposable
    {
        private readonly ILevelGoalModel _levelGoalModel;
        private readonly IHudView _hudView;
        private readonly IAnimatedFXViewFactory _animatedFXViewFactory;
        private readonly List<(ImageFXView, GoalSlotView)> _fxViews = new();

        public HudPresenter(ILevelGoalModel levelGoalModel, IHudView hudView, IAnimatedFXViewFactory animatedFXViewFactory)
        {
            _levelGoalModel = levelGoalModel;
            _hudView = hudView;
            _animatedFXViewFactory = animatedFXViewFactory;
        }

        public void Initialize()
        {
            _levelGoalModel.OnMoveCountUpdate += OnMoveCountUpdate;
            _levelGoalModel.OnGoalCountUpdate += CreateFxViews;
            _levelGoalModel.OnGoalCountUpdateComplete += AnimateFxViews;
        }

        private void CreateFxViews(ObstacleType obstacleType, Vector3 objPos, Vector2 uiSizeDelta)
        {
            if (!_hudView.SlotByType.TryGetValue(obstacleType, out var slotView))
            {
                EditorLogger.LogError($"{obstacleType} slot view not found!");
                return;
            }

            if (slotView.IsCollectible)
            {
                var animatedFX = _animatedFXViewFactory.GetAnimatedFX<ImageFXView>(_hudView.GoalFxHolder, objPos, slotView.GetIcon(), uiSizeDelta,false);
                
                _fxViews.Add((animatedFX, slotView));
            }
            else
            {
                slotView.DecreaseGoalCount();
            }
        }
        
        private void AnimateFxViews()
        {
            for (var i = 0; i < _fxViews.Count; i++)
            {
                var (animatedFX, slotView) = _fxViews[i];
                var delay = i * .05f;
                
                animatedFX.Activate();
                
                animatedFX.MoveTo(slotView.transform.position, .75f, delay, Ease.InBack)
                            .SetSize(slotView.GetSize(), .75f, delay)
                            .OnMoveComplete(() => OnAnimComplete(animatedFX, slotView));
            }
            
            _fxViews.Clear();
        }

        private void OnAnimComplete(ImageFXView fxView, GoalSlotView slotView)
        {
            _animatedFXViewFactory.ReleaseAnimatedFX(fxView);
            slotView.DecreaseGoalCount();
        }

        private void OnMoveCountUpdate()
        {
            var moveCount = _levelGoalModel.MoveCount;
            _hudView.SetMoveCount(moveCount);
        }

        public void Dispose()
        {
            _levelGoalModel.OnGoalCountUpdate -= CreateFxViews;
            _levelGoalModel.OnGoalCountUpdateComplete -= AnimateFxViews;
            _levelGoalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}