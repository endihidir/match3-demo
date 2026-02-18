using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.Scene.Services;
using Core.Pool.Services;
using DG.Tweening;
using Game.Level.Models;
using Game.Level.Services;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrappers
{
    public class AppBootstrapper : IInitializable
    {
        private const int TweenCapacity = 2000;
        private const int SequenceCapacity = 500;
        
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
            if(!_sceneLoadService.IsInAnyGameScene) return;

            await _sceneLoadService.InitBootSceneAsync();
            
            var isInitialized = await _levelDataService.InitializeAsync();
            
            if (!isInitialized) return;
            
            _levelProgressionModel.Initialize(_levelDataService.LevelSize);
            
            _objectPoolService.Initialize();
            
            DOTween.SetTweensCapacity(TweenCapacity, SequenceCapacity);
            
            await _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene);
        }
    }
}