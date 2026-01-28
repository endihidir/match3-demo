using System;
using System.Linq;
using Core.Item;
using Core.Models;
using Core.UI;
using Core.Utils;
using VContainer.Unity;

namespace Core.Presenters
{
    public sealed class HudPresenter : IInitializable, IDisposable
    {
        private readonly ILevelGoalModel _levelGoalModel;
        private readonly IHudView _hudView;

        public HudPresenter(ILevelGoalModel levelGoalModel, IHudView hudView)
        {
            _levelGoalModel = levelGoalModel;
            _hudView = hudView;
        }

        public void Initialize()
        {
            _levelGoalModel.OnGoalCountUpdate += OnGoalCountUpdate;
            _levelGoalModel.OnMoveCountUpdate += OnMoveCountUpdate;
        }
        
        private void OnGoalCountUpdate(ObstacleType obstacleType, int count)
        {
            var slotView = _hudView.GoalSlotViews.FirstOrDefault(x => x.ObstacleType == obstacleType);

            if (!slotView)
            {
                EditorLogger.LogError($"{obstacleType} slot view not found!");
                return;
            }
            
            slotView.SetGoalCount(count);
        }

        private void OnMoveCountUpdate(int moveCount) => _hudView.SetMoveCount(moveCount);
        
        public void Dispose()
        {
            _levelGoalModel.OnGoalCountUpdate -= OnGoalCountUpdate;
            _levelGoalModel.OnMoveCountUpdate -= OnMoveCountUpdate;
        }
    }
}