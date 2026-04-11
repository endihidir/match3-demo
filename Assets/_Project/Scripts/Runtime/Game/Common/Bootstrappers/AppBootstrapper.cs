using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.Scene.Services;
using Core.Pool.Services;
using DG.Tweening;
using Game.Configs;
using Game.Level.Models;
using Game.Level.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.Bootstrappers
{
    public class AppBootstrapper : IInitializable
    {
        [Inject] private readonly AppSettingsSO _appSettings;
        [Inject] private readonly IObjectResolver _objectResolver;
        [Inject] private readonly ISceneLoadService _sceneLoadService;
        [Inject] private readonly IObjectPoolService _objectPoolService;
        [Inject] private readonly ILevelDataService _levelDataService;
        [Inject] private readonly ILevelProgressionModel _levelProgressionModel;
        
        public void Initialize()
        {
            InitGame().Forget();
        }

        private async UniTask InitGame()
        {
            Application.targetFrameRate = _appSettings.TargetFrameRate;
            
            Input.multiTouchEnabled = _appSettings.IsMultitouchEnabled;
            
            if(!_sceneLoadService.IsInAnyGameScene) return;

            await _sceneLoadService.InitBootSceneAsync();
            
            var isInitialized = await _levelDataService.InitializeAsync();
            
            if (!isInitialized) return;
            
            _levelProgressionModel.Initialize(_levelDataService.LevelSize);
            
            _objectPoolService.Initialize();
            
            DOTween.SetTweensCapacity(_appSettings.TweenCapacity, _appSettings.SequenceCapacity);
            
            await _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene);
        }
    }
}