using Cysharp.Threading.Tasks;
using Game.Level.Data;

namespace Game.Level.Services
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