using Game.Grid.Item;
using Game.Level.Models;
using Game.Views;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class LevelGoalProgressHandler : ILevelGoalProgressHandler
    {
        private readonly IGridView _gridView;
        private readonly ILevelGoalModel _levelGoalModel;

        public LevelGoalProgressHandler(IGridView gridView, ILevelGoalModel levelGoalModel)
        {
            _gridView = gridView;
            _levelGoalModel = levelGoalModel;
        }
        
        public void ProgressMove() => _levelGoalModel.ConsumeMove();

        public void ProgressGoal(IDamageableGridObject damageableGridObject, Vector2Int coord, Vector2 spriteSize)
        {
            var worldPos = _gridView.GridToWorld(coord);
            var rectSize = _gridView.SpriteToRectSize(spriteSize);
            _levelGoalModel.ProgressGoal(damageableGridObject, worldPos, rectSize);
        }
    }
}