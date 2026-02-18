using Game.Grid.Item;

namespace Game.Grid.Handlers
{
    public interface IGridCheatHandler
    {
        void Cleanup<T>() where T : BaseGridObject;
        void GenerateItemAtMousePos(ItemType type);
        void GenerateBoosterAtMousePos(BoosterType boosterType);
        void GenerateObstacleAtMousePos(ObstacleType type);
        void RemoveAtMousePos();
        void ForceRefill();
    }
}