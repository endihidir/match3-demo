using Core.Item;

namespace Core.Handlers
{
    public interface IGridCheatHandler
    {
        void Cleanup<T>() where T : BaseGridObject;
        void GenerateBoosterAtMousePos(BoosterType boosterType);
        void GenerateObstacleAtMousePos(ObstacleType type);
        void RemoveAtMousePos();
        void ForceRefill();
    }
}