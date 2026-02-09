using AYellowpaper.SerializedCollections;
using Core.Item;
using UnityEngine;

namespace Core.UI
{
    public interface IHudView
    {
        public Transform GoalFxHolder { get; }
        void Initialize(GoalSlotView[] goalSlotViews, int moveCount);
        void SetMoveCount(int moveCount);
        bool TryGetGoalSlotView(ObstacleType obstacleType, out GoalSlotView goalSlotView);
        void DecreaseGoalCount(ObstacleType obstacleType, bool useBounceAnim = true);
    }
}