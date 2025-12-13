using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.Level;
using Core.SceneService;
using Core.Pool;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class AppBootstrapper : IInitializable
    {
        [Inject] private readonly IObjectResolver _objectResolver;
        [Inject] private readonly ISceneLoadService _sceneLoadService;
        [Inject] private readonly IObjectPoolService _objectPoolService;
        [Inject] private readonly ILevelDataBootState _levelDataBootState;
        
        public void Initialize()
        {
            InitGame().Forget();
        }

        private async UniTask InitGame()
        {
            if(!_sceneLoadService.IsInAnyGameScene) return;

            await _sceneLoadService.InitBootSceneAsync();
            
            await _levelDataBootState.WaitUntilInitializedAsync();
            
            _objectPoolService.Initialize();
            
            await _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene);
        }
    }
}