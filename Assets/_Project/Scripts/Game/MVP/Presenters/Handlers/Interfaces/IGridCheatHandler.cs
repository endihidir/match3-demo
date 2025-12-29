using Core.Item;

namespace Core.Handlers
{
    public interface IGridCheatHandler
    {
        void CleanupBoosters();
        void GenerateBoosterAtMousePos(BoosterType boosterType);
    }
}