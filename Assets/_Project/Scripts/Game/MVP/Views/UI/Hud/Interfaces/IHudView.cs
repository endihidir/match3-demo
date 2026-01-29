using AYellowpaper.SerializedCollections;
using Core.Item;
using UnityEngine;

namespace Core.UI
{
    public interface IHudView
    {
        public Transform GoalFxHolder { get; }
        SerializedDictionary<ObstacleType, GoalSlotView> SlotByType { get; }
        void Initialize(GoalSlotView[] goalSlotViews, int moveCount);
        void SetMoveCount(int moveCount);
    }
}