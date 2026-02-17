using System;
using Core.Generated;
using Core.Level;
using Core.SceneService;
using Core.Views;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Core.Presenters
{
    public sealed class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IMainMenuView _mainMenuView;
        
        public MainMenuPresenter(ILevelDefinitionProvider levelDefinitionProvider, ISceneLoadService sceneLoadService, IMainMenuView mainMenuView)
        {
            _levelDefinitionProvider = levelDefinitionProvider;
            _sceneLoadService = sceneLoadService;
            _mainMenuView = mainMenuView;
        }
        
        public void Initialize()
        {
            _mainMenuView.SetLevelNumber(_levelDefinitionProvider.GetLevelNumber());
            
            AddListeners();
        }

        private void AddListeners()
        {
            _mainMenuView.ClickedEvent.AddListener(OnClickPlayButton);
        }

        private void OnClickPlayButton() => LoadSceneAsync().Forget();
        private async UniTask LoadSceneAsync()
        {
            await _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.GameScene, true);
        }

        private void RemoveListeners()
        {
            _mainMenuView.ClickedEvent.RemoveListener(OnClickPlayButton);
        }
        
        public void Dispose()
        {
            RemoveListeners();
        }
    }
}