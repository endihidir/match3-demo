using System;
using Core.Generated;
using Core.Scene.Services;
using Cysharp.Threading.Tasks;
using Game.Level.Services;
using Game.Menu.Views;
using VContainer.Unity;

namespace Game.Presenters
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