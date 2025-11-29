using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Core.Configs;
using Core.Generated;
using Core.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Core.SceneService
{
    public interface ISceneLoadService
    {
        event Action<bool, SceneType> OnBeforeSceneLoad;
        event Func<bool, SceneType, UniTask>  OnAfterScenesLoad; 
        event Action<string> OnSceneLoad; 
        event Action<SceneType> OnScenesReady;
        event Action<string> OnSceneUnloaded; 
        LoadingProgress LoadingProgress { get; }
        bool IsInBootScene { get; }
         bool IsInAnyGameScene { get; }
        UniTask EnsureBootSceneLoadedAsync();
        UniTask LoadBootSceneAsync();
        UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene = false, bool reloadDupScenes = false);
    }

    public class SceneLoadService : ISceneLoadService
    {
        private readonly SceneLoadServiceConfig _sceneLoadConfig;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        public event Action<bool, SceneType> OnBeforeSceneLoad;
        public event Func<bool, SceneType, UniTask> OnAfterScenesLoad;
        public event Action<string> OnSceneLoad;
        public event Action<SceneType> OnScenesReady;
        public event Action<string> OnSceneUnloaded;
        public LoadingProgress LoadingProgress { get; }

        public bool IsInBootScene
        {
            get
            {
                var current = SceneManager.GetActiveScene().name;

                var firstScene = BuildSettingsUtils.GetFirstBuildSceneName();

                return current == firstScene;
            }
        }

        public bool IsInAnyGameScene {
            get
            {
                var current = SceneManager.GetActiveScene().name;

                var firstScene = BuildSettingsUtils.GetFirstBuildSceneName();
                var sceneNames = new List<string> { firstScene };

                var sceneTypes = Enum.GetValues(typeof(SceneType));
                
                foreach (var type in sceneTypes)
                {
                    var sceneType = (SceneType)type;
                    
                    sceneNames.Add(SceneIdLookup.GetSceneId(sceneType));
                }
                
                return sceneNames.Contains(current);
            }
        }

        public SceneLoadService(AppConfigContainer appConfigContainer)
        {
            _sceneLoadConfig = appConfigContainer.sceneLoadServiceConfig;

            _handleGroup = new AsyncOperationHandleGroup(10);

            _operationGroup = new AsyncOperationGroup(10);

            LoadingProgress = new LoadingProgress();
        }
        
        public async UniTask EnsureBootSceneLoadedAsync()
        {
            if (IsInBootScene)
            {
                return;
            }

            await LoadBootSceneAsync();
        }

        public async UniTask LoadBootSceneAsync()
        {
            var current = SceneManager.GetActiveScene().name;

            var firstScene = BuildSettingsUtils.GetFirstBuildSceneName();

            if (current != firstScene)
            {
                await SceneManager.LoadSceneAsync(firstScene, LoadSceneMode.Single);
            }
        }

        public async UniTask LoadSceneAsync(SceneType sceneType, bool useLoadingScene = false, bool reloadDupScenes = false)
        {
            var loadedScenes = new List<string>();

            var sceneId = SceneIdLookup.GetSceneId(sceneType);

            OnBeforeSceneLoad?.Invoke(useLoadingScene, sceneType);

            var sceneCount = SceneManager.sceneCount;

            for (var i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(SceneManager.GetSceneAt(i).name);
            }

            await UnloadSceneAsync();

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

                OnSceneLoad?.Invoke(sceneReference.Name);
            }

            while (!_operationGroup.IsDone || !_handleGroup.IsDone)
            {
                var avg = CombinedProgress(_operationGroup, _handleGroup);

                LoadingProgress?.Report(avg);

                await UniTask.Delay(100);
            }

            if (_sceneLoadConfig.TryGetActiveSceneById(sceneId, out var activeScene) && activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }

            LoadingProgress?.Report(1f);

            if (OnAfterScenesLoad != null)
            {
                await InvokeAfterScenesLoadAll(useLoadingScene, sceneId);
            }

            OnScenesReady?.Invoke(sceneType);
        }

        private static float CombinedProgress(AsyncOperationGroup op, AsyncOperationHandleGroup handle)
        {
            var nOp = op.Operations.Count;
            var nHd = handle.Handles.Count;

            if (nOp == 0 && nHd == 0) return 0f;
            if (nOp == 0) return handle.Progress;
            if (nHd == 0) return op.Progress;

            return (op.Progress * nOp + handle.Progress * nHd) / (nOp + nHd);
        }

        private async UniTask InvokeAfterScenesLoadAll(bool useUI, string sceneName)
        {
            var list = OnAfterScenesLoad?.GetInvocationList();

            if (list == null || list.Length == 0) return;

            var tasks = new UniTask[list.Length];

            for (int i = 0; i < list.Length; i++)
            {
                var fn = (Func<bool, string, UniTask>)list[i];
                tasks[i] = SafeCall(fn, useUI, sceneName);
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask SafeCall(Func<bool, string, UniTask> fn, bool useUI, string sceneName)
        {
            try
            {
                await fn(useUI, sceneName);
            }

            catch (Exception ex)
            {
                EditorLogger.LogError(ex);
            }
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
                
                OnSceneUnloaded?.Invoke(sceneName);
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
                
                OnSceneUnloaded?.Invoke(sceneName);
            }

            while (!_operationGroup.IsDone)
            {
                await UniTask.Delay(100);
            }
            
            _operationGroup.Operations.Clear();

            await Resources.UnloadUnusedAssets();
        }
    }
    
    public class LoadingProgress : IProgress<float>
    {
        public event Action<float> Progressed;
        
        private const float RATIO = 1f;
        public void Report(float value)
        {
            Progressed?.Invoke(value / RATIO);
        }
    }
}