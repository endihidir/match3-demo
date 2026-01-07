using Core.Item;

namespace Core.Handlers
{
    public interface IGridCheatHandler
    {
        void CleanupBoosters();
        void GenerateBoosterAtMousePos(BoosterType boosterType);
        void GenerateObstacleAtMousePos(ObstacleType type);
        void RemoveAtMousePos();
        void ForceRefill();
    }
}