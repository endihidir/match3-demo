namespace Core.Services
{
    public interface IGameplaySetupService
    {
        void SetupGameplay();
        void ResetGameplay();
        void ReleaseFactories();
    }
}