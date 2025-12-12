using System;
using Core.Generated;
using Core.Models;
using Core.SceneService;
using Core.Utils;
using Core.Views;
using Cysharp.Threading.Tasks;

namespace Core.Presenters
{
    public interface IMainMenuPresenter : IDisposable
    {
        IMainMenuPresenter Initialize(IPlayButtonView playButtonView);
    }
    
    public sealed class MainMenuPresenter : IMainMenuPresenter
    {
        private readonly ILevelProgressReadModel _levelProgressReadModel;
        private readonly ISceneLoadService _sceneLoadService;
        
        private IPlayButtonView _playButtonView;
        
        public MainMenuPresenter(ISceneLoadService sceneLoadService, ILevelProgressReadModel levelProgressReadModel)
        {
            _sceneLoadService = sceneLoadService;
            _levelProgressReadModel = levelProgressReadModel;
        }
        
        public IMainMenuPresenter Initialize(IPlayButtonView playButtonView)
        {
            _playButtonView = playButtonView;
            
            _playButtonView.SetText($"Level {_levelProgressReadModel.DisplayLevelNumber}");
            
            AddListeners();
            
            return this;
        }

        private void AddListeners()
        {
            _playButtonView.ClickedEvent.AddListener(OnClickPlayButton);
        }

        private void OnClickPlayButton() => LoadSceneAsync().Forget();
        private async UniTask LoadSceneAsync()
        {
            await _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.GameScene, true);
        }

        private void RemoveListeners()
        {
            _playButtonView.ClickedEvent.RemoveListener(OnClickPlayButton);
        }
        
        public void Dispose()
        {
            RemoveListeners();
        }
    }
}