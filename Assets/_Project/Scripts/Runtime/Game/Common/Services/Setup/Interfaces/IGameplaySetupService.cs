using Game.Grid.Item;

namespace Game.Services
{
    public interface IGameplaySetupService
    {
        void SetupGameplay();
        void ResetGameplay();
        void ReleaseFactories();
        void AddNewGoal(GridObjectType goalType, int count);
    }
}