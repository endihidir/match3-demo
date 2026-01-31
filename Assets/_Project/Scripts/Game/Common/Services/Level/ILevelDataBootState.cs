using Cysharp.Threading.Tasks;

namespace Core.Level
{
    public interface ILevelDataBootState
    {
        bool IsInitialized { get; } 
        UniTask WaitUntilInitializedAsync(); 
    }
}