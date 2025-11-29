using System;
using Core.Generated;
using Core.Models;
using Core.MVPContext;
using Core.MVPContext.Interfaces;
using Core.SceneService;
using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Presenters
{
    public interface IMainMenuPresenter : IPresenter, IDisposable
    {
        IMainMenuPresenter Initialize(IMainMenuView mainMenuView, IPlayButtonView playButtonView);
    }
    
    public sealed class MainMenuPresenter : IMainMenuPresenter
    {
        private readonly ILevelProgressReadModel _levelProgressReadModel;
        private readonly ISceneLoadService _sceneLoadService;
        
        public MainMenuPresenter(ISceneLoadService sceneLoadService, IGlobalModelService globalModelService)
        {
            _sceneLoadService = sceneLoadService;
            _levelProgressReadModel = globalModelService.Resolve<LevelProgressModel>();
        }
        
        private IMainMenuView _mainMenuView;
        private IPlayButtonView _playButtonView;
        
        public IMainMenuPresenter Initialize(IMainMenuView mainMenuView, IPlayButtonView playButtonView)
        {
            _mainMenuView = mainMenuView;
            _playButtonView = playButtonView;
            
            playButtonView.SetText($"Level {_levelProgressReadModel.DisplayLevelNumber}");
            
            AddListeners();
            
            return this;
        }

        private void AddListeners()
        {
            _playButtonView.Button.onClick.AddListener(OnClickPlayButton);
        }

        private void OnClickPlayButton() => LoadSceneAsync().Forget();
        private async UniTask LoadSceneAsync()
        {
            await _sceneLoadService.LoadSceneAsync(SceneType.GameScene, true);
        }

        private void RemoveListeners()
        {
            _playButtonView.Button.onClick.RemoveListener(OnClickPlayButton);
        }
        
        public void Dispose()
        {
            RemoveListeners();
        }
    }
}