using AYellowpaper.SerializedCollections;
using Core.Item;

namespace Core.UI
{
    public interface IHudView
    {
        public SerializedDictionary<ObstacleType, GoalSlotView> SlotByType { get; }
        void Initialize(GoalSlotView[] goalSlotViews, int moveCount);
        void SetMoveCount(int moveCount);
    }
}