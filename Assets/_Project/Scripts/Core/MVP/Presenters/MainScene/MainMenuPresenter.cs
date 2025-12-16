using System;
using Core.Generated;
using Core.SceneService;
using Core.Views;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Core.Presenters
{
    public sealed class MainMenuPresenter : IInitializable, IDisposable
    {
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IPlayButtonView _playButtonView;
        
        public MainMenuPresenter(ISceneLoadService sceneLoadService, IPlayButtonView playButtonView)
        {
            _sceneLoadService = sceneLoadService;
            _playButtonView = playButtonView;
        }
        
        public void Initialize()
        {
            AddListeners();
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