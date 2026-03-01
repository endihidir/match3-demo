using System;
using Game.Configs;
using Game.Grid.Item;
using Game.HUD.Handlers;
using Game.HUD.Handlers.Data;
using Game.Level.Models;
using Game.Views;
using UnityEngine;

namespace Game.Level.Handlers
{
    public sealed class LevelGoalHandler : ILevelGoalHandler
    {
        private readonly IGridView _gridView;
        private readonly IHudView _hudView;
        private readonly ILevelGoalModel _levelGoalModel;
        private readonly IGoalSlotHandler _goalSlotHandler;
        private readonly GridConfigContainerSO _gridConfigContainer;
        public event Action<GoalCollectedData> OnGoalCollected;

        public LevelGoalHandler(IGridView gridView, IHudView hudView, ILevelGoalModel levelGoalModel, IGoalSlotHandler goalSlotHandler, GridConfigContainerSO gridConfigContainer)
        {
            _gridView = gridView;
            _hudView = hudView;
            _levelGoalModel = levelGoalModel;
            _goalSlotHandler = goalSlotHandler;
            _gridConfigContainer = gridConfigContainer;
        }

        public void ProgressMove() => _levelGoalModel.ConsumeMove();

        public void ProgressGoal(BaseGridObject gridObject)
        {
            var type = gridObject.ObjectType;
            if(_levelGoalModel.IsGoalComplete(type)) return;
            _levelGoalModel.ProgressGoal(type);
            
            if (!TryBuildGoalCollectedData(type, gridObject.Coord, gridObject.SpriteRenderer.size, out var collectedData)) return;
            OnGoalCollected?.Invoke(collectedData);
        }
        
        public void RegisterGoal(GridObjectType goalType, int count)
        {
            _levelGoalModel.AddGoal(goalType, count);
            var goals = _levelGoalModel.Goals;
            _goalSlotHandler.PopulateSlotViews(goals);
            _hudView.Initialize(goals.Count, _levelGoalModel.MoveCount);
        }

        private bool TryBuildGoalCollectedData(GridObjectType gridObjectType, Vector2Int coord, Vector2 spriteSize, out GoalCollectedData data)
        {
            var gridObjectData = _gridConfigContainer.GetConfigData(gridObjectType);
            
            if (!gridObjectData)
            {
                data = default;
                return false;
            }
            
            var worldPos = _gridView.GridToWorld(coord);
            var rectSize = _gridView.SpriteToRectSize(spriteSize);
            data = new GoalCollectedData(gridObjectType, gridObjectData, worldPos, rectSize);
            return true;
        }
    }
}