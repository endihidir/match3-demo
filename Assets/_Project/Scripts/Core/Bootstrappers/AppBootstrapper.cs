using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.Level;
using Core.Models;
using Core.SceneService;
using Core.Pool;
using DG.Tweening;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrappers
{
    public class AppBootstrapper : IInitializable
    {
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
            
            DOTween.SetTweensCapacity(2000, 500);
            
            await _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene);
        }
    }
}