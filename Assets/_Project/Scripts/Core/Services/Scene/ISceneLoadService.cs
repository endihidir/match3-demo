using Core.Generated;
using Cysharp.Threading.Tasks;

namespace Core.SceneService
{
    public interface ISceneLoadService
    {
        bool IsInAnyGameScene { get; }
        bool IsInBootScene { get; }
        UniTask InitBootSceneAsync();
        UniTask LoadSceneGroupAsync(SceneGroupType groupType, bool useTransitionView = false, bool reloadDupScenes = false);
    }
}