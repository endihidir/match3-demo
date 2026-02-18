using Cysharp.Threading.Tasks;

namespace Core.Level
{
    public interface ILevelDataService
    {
        bool IsInitialized { get; }
        int LevelSize { get; }
        UniTask<bool> InitializeAsync();
        LevelDefinition GetLevelDefinition(int index);
        UniTask<LevelDefinition> LoadLevelDefinitionAsync(int level);
    }
}