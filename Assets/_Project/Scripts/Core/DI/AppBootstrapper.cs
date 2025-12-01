using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.Level;
using Core.SceneService;
using Core.Pool;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class AppBootstrapper : IInitializable
    {
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IObjectPoolService _objectPoolService;
        private readonly ILevelDataService _levelDataService;

        private AppBootstrapper(ISceneLoadService sceneLoadService, IObjectPoolService objectPoolService, ILevelDataService levelDataService)
        {
            _sceneLoadService = sceneLoadService;
            _objectPoolService = objectPoolService;
            _levelDataService = levelDataService;
        }

        public void Initialize()
        {
            InitGame().Forget();
        }

        private async UniTask InitGame()
        {
            if(!_sceneLoadService.IsInAnyGameScene) return;

            await _sceneLoadService.LoadBootSceneAsync();
            
            await _levelDataService.WaitUntilInitializedAsync();
            
            _objectPoolService.Initialize();
            
            await _sceneLoadService.LoadSceneAsync(SceneType.MenuScene);
        }
    }
}