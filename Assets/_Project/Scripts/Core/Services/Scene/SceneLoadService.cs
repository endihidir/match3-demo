using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Core.Configs;
using Core.Generated;
using Core.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Core.SceneService
{
    public interface ISceneLoadEvents
    {
        event Action<bool> OnBeforeTransition;
        event Action OnScenesUnload; 
        ProgressHandler Progress { get; }
        event Action OnBeforeScenesActivate; 
        event Func<UniTask>  OnBeforeTransitionOut; 
        event Action OnTransitionComplete;
    }

    public interface ISceneLoadInfo
    {
        SceneType CurrentSceneType { get; }
        float ProgressSpeed { get; }
    }
    
    public interface ISceneLoadService
    {
        bool IsInAnyGameScene { get; }
        bool IsInBootScene { get; }
        UniTask LoadBootSceneAsync();
        UniTask LoadSceneAsync(SceneType sceneType, bool useTransitionView = false, bool reloadDupScenes = false);
    }

    public class SceneLoadService : ISceneLoadService, ISceneLoadEvents, ISceneLoadInfo, ITickable
    {
        private readonly SceneLoadServiceConfig _sceneLoadConfig;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        public event Action<bool> OnBeforeTransition;
        public event Action OnScenesUnload;
        public event Action OnBeforeScenesActivate;
        public event Func<UniTask> OnBeforeTransitionOut;
        public event Action OnTransitionComplete;
        public float ProgressSpeed { get; }
        public SceneType CurrentSceneType { get; private set; }
        public ProgressHandler Progress { get; }

        public bool IsInAnyGameScene {
            get
            {
                var current = SceneManager.GetActiveScene().name;
                var firstScene = BuildSettingsUtils.GetFirstBuildSceneName();
                var sceneNames = new List<string> { firstScene };
                var sceneTypes = Enum.GetValues(typeof(SceneType));
                sceneNames.AddRange(from SceneType sceneType in sceneTypes select SceneIdLookup.GetSceneId(sceneType));
                return sceneNames.Contains(current);
            }
        }

        public bool IsInBootScene
        {
            get
            {
                var current = SceneManager.GetActiveScene().name;
            
                var firstScene = BuildSettingsUtils.GetFirstBuildSceneName();

                return current == firstScene;
            }
        }

        public SceneLoadService(AppConfigContainer appConfigContainer)
        {
            _sceneLoadConfig = appConfigContainer.sceneLoadServiceConfig;
            _handleGroup = new AsyncOperationHandleGroup(10);
            _operationGroup = new AsyncOperationGroup(10);
            Progress = new ProgressHandler();
            ProgressSpeed = _sceneLoadConfig.ProgressSpeed;
        }

        public async UniTask LoadBootSceneAsync()
        {
            var firstScene = BuildSettingsUtils.GetFirstBuildSceneName();

            if (!IsInBootScene)
            {
                await SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Single);
            }
        }

        public async UniTask LoadSceneAsync(SceneType sceneType, bool useTransitionView = false, bool reloadDupScenes = false)
        {
            var loadedScenes = new List<string>();

            var sceneId = SceneIdLookup.GetSceneId(sceneType);
            
            OnBeforeTransition?.Invoke(useTransitionView);

            var sceneCount = SceneManager.sceneCount;
            
            var currentSceneId = SceneManager.GetActiveScene().name;

            for (var i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(currentSceneId);
            }
            
            CurrentSceneType = SceneIdLookup.GetSceneType(currentSceneId);
            
            await UnloadSceneAsync();
                 
            OnScenesUnload?.Invoke();
            
            var sceneGroup = _sceneLoadConfig.GetSceneData(sceneId);
            
            for (int i = 0; i < sceneGroup.Count; i++)
            {
                var sceneReference = sceneGroup[i];

                if (!reloadDupScenes && loadedScenes.Contains(sceneReference.Name)) continue;

                if (sceneReference.State == SceneReferenceState.Regular)
                {
                    var operation = SceneManager.LoadSceneAsync(sceneReference.Path, LoadSceneMode.Additive);

                    _operationGroup.Operations.Add(operation);
                }
                else if (sceneReference.State == SceneReferenceState.Addressable)
                {
                    var sceneHandle = Addressables.LoadSceneAsync(sceneReference.Path, LoadSceneMode.Additive);

                    _handleGroup.Handles.Add(sceneHandle);
                }
            }

            while (!_operationGroup.IsDone || !_handleGroup.IsDone)
            {
                var avg = AsyncOperationUtils.CombinedProgress(_operationGroup, _handleGroup);

                Progress?.Report(avg);

                await UniTask.Delay(100);
            }
            
            OnBeforeScenesActivate?.Invoke();
            
            if (_sceneLoadConfig.TryGetActiveSceneById(sceneId, out var activeScene) && activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }

            Progress?.Report(1f);

            CurrentSceneType = sceneType;
            
            if (OnBeforeTransitionOut != null)
            {
                await OnBeforeTransitionOut.Invoke();
            }
            
            OnTransitionComplete?.Invoke();
        }

        private async UniTask UnloadSceneAsync()
        {
            var bootSceneName = BuildSettingsUtils.GetFirstBuildSceneName();
            
            foreach (var handle in _handleGroup.Handles.ToArray())
            {
                if (!handle.IsValid()) continue;

                var sceneName = handle.Result.Scene.name;
                
                if (sceneName == bootSceneName) continue;

                await Addressables.UnloadSceneAsync(handle);
            }

            _handleGroup.Handles.Clear();

            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                
                if (!sceneAt.isLoaded) continue;

                var sceneName = sceneAt.name;
                
                if (sceneName == bootSceneName) continue;

                var op = SceneManager.UnloadSceneAsync(sceneAt);
                
                if (op == null) continue;

                _operationGroup.Operations.Add(op);
            }

            while (!_operationGroup.IsDone)
            {
                await UniTask.Delay(100);
            }
            
            _operationGroup.Operations.Clear();

            await Resources.UnloadUnusedAssets();
        }

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(CurrentSceneType == SceneType.MenuScene) return;
                
                LoadSceneAsync(SceneType.MenuScene, true).Forget();
            }
        }
    }
}