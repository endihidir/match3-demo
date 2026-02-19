using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Game.Configs;
using Core.Generated;
using Core.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Core.Scene.Services
{
    public class SceneLoadService : ISceneLoadService, ISceneLoadState
    {
        private readonly SceneLoadServiceConfigSO _sceneLoadConfigSo;
        private readonly AsyncOperationHandleGroup _handleGroup;
        private readonly AsyncOperationGroup _operationGroup;
        private readonly string _firstSceneName;
        public event Action OnLoadStart;
        public event Action OnScenesUnload;
        public event Action OnScenesLoad;
        public event Action OnScenesActivate;
        public event Func<UniTask> OnTransitionOut;
        public event Action OnLoadComplete;
        public bool IsTransitionViewActivated { get; private set; }
        public ProgressHandler Progress { get; }
        
        private SceneGroupType CurrentSceneGroupType { get; set; }
        public float ProgressSpeed { get; }
        private string ActiveSceneName => SceneManager.GetActiveScene().name;

        public bool IsInAnyGameScene 
        {
            get
            {
                var sceneNames = new List<string> { _firstSceneName };
                var sceneGroupTypes = Enum.GetValues(typeof(SceneGroupType));
                sceneNames.AddRange(from SceneGroupType sceneGroupType in sceneGroupTypes select SceneIdLookup.GetSceneGroupId(sceneGroupType));
                return sceneNames.Contains(ActiveSceneName);
            }
        }

        public bool IsInBootScene => ActiveSceneName.Equals(_firstSceneName);

        public SceneLoadService(SceneLoadServiceConfigSO sceneLoadServiceConfigSo)
        {
            _sceneLoadConfigSo = sceneLoadServiceConfigSo;
            _handleGroup = new AsyncOperationHandleGroup(10);
            _operationGroup = new AsyncOperationGroup(10);
            _firstSceneName = BuildSettingsUtils.GetFirstBuildSceneName();
            
            Progress = new ProgressHandler();
            ProgressSpeed = _sceneLoadConfigSo.ProgressSpeed;
        }

        public async UniTask InitBootSceneAsync()
        {
            if (IsInBootScene) return;
           
            await SceneManager.LoadSceneAsync(_firstSceneName, LoadSceneMode.Single);
        }

        public async UniTask LoadSceneGroupAsync(SceneGroupType groupType, bool useTransitionView = false, bool reloadDupScenes = false)
        {
            IsTransitionViewActivated = useTransitionView;
            
            OnLoadStart?.Invoke();
            
            var sceneCount = SceneManager.sceneCount;
          
            var loadedScenes = new List<string>();
            
            for (var i = 0; i < sceneCount; i++)
            {
                loadedScenes.Add(ActiveSceneName);
            }
            
            CurrentSceneGroupType = SceneIdLookup.GetSceneGroupType(ActiveSceneName);
            
            await UnloadSceneAsync();
                 
            OnScenesUnload?.Invoke();
            
            var sceneId = SceneIdLookup.GetSceneGroupId(groupType);
            
            var sceneGroup = _sceneLoadConfigSo.GetSceneGroupData(sceneId);
            
            foreach (var sceneReference in sceneGroup)
            {
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
            
            OnScenesLoad?.Invoke();
            
            if (_sceneLoadConfigSo.TryGetActiveSceneBy(sceneId, out var activeScene) && activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }
            
            OnScenesActivate?.Invoke();

            Progress?.Report(1f);
            
            if (IsTransitionViewActivated && OnTransitionOut != null)
            {
                await OnTransitionOut.Invoke();
            }
            
            CurrentSceneGroupType = groupType;
            
            OnLoadComplete?.Invoke();
        }
        
        public bool IsLoadedSceneGroup(SceneGroupType sceneGroupType) => CurrentSceneGroupType == sceneGroupType;

        private async UniTask UnloadSceneAsync()
        {
            foreach (var handle in _handleGroup.Handles)
            {
                if (!handle.IsValid()) continue;

                var sceneName = handle.Result.Scene.name;
                
                if (sceneName.Equals(_firstSceneName)) continue;

                await Addressables.UnloadSceneAsync(handle);
            }

            _handleGroup.Handles.Clear();

            for (var i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var sceneAt = SceneManager.GetSceneAt(i);
                
                if (!sceneAt.isLoaded) continue;

                var sceneName = sceneAt.name;
                
                if (sceneName.Equals(_firstSceneName)) continue;

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
    }
}