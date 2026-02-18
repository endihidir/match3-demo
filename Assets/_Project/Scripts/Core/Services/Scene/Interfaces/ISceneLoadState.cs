using System;
using Core.Generated;
using Cysharp.Threading.Tasks;

namespace Core.SceneService
{
    public interface ISceneLoadState
    {
        event Action OnLoadStart;
        event Action OnScenesUnload; 
        event Action OnScenesLoad; 
        event Action OnScenesActivate; 
        event Func<UniTask> OnTransitionOut; 
        event Action OnLoadComplete;
        ProgressHandler Progress { get; }
        float ProgressSpeed { get; }
        bool IsTransitionViewActivated { get; }
        bool IsLoadedSceneGroup(SceneGroupType sceneGroupType);
    }
}