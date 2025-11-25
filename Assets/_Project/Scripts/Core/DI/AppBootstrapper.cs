using Cysharp.Threading.Tasks;
using Core.Generated;
using Core.SceneService;
using Core.Pool;
using Core.SaveSystem;
using UnityEngine;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class AppBootstrapper : IInitializable
    {
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IObjectPoolService _objectPoolService;

        private AppBootstrapper(ISceneLoadService sceneLoadService, IObjectPoolService objectPoolService)
        {
            _sceneLoadService = sceneLoadService;
            _objectPoolService = objectPoolService;
        }

        public void Initialize()
        {
            InitGame().Forget();
        }

        private async UniTask InitGame()
        {
            if(!_sceneLoadService.IsInAnyGameScene) return;
            
            if (!_sceneLoadService.IsInStartScene)
            {
                await _sceneLoadService.LoadBootSceneAsync();
            }

            CreateSaveDispatcher();
            
            _objectPoolService.Initialize();
            
            await _sceneLoadService.LoadSceneAsync(SceneType.MenuScene);
        }

        private static void CreateSaveDispatcher()
        {
            if (!Object.FindObjectOfType<SaveDispatcher>(true))
            {
                new GameObject(nameof(SaveDispatcher)).AddComponent<SaveDispatcher>();
            }
        }
    }
}