using System;
using Core.Models;
using Core.SceneService;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
{
    public class SceneTransitionPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly ISceneLoadState _sceneLoadState;
        private readonly ISceneTransitionModel _transitionModel;
        private readonly ISceneTransitionView _transitionView;
        private bool _isTransitionViewEnabled = false;
        public SceneTransitionPresenter(ISceneLoadState sceneLoadState, ISceneTransitionModel sceneTransitionTransitionModel, ISceneTransitionView transitionView)
        {
            _sceneLoadState = sceneLoadState;
            _transitionModel = sceneTransitionTransitionModel;
            _transitionView = transitionView;
        }

        public void Initialize()
        {
            _sceneLoadState.OnLoadStart += OnBeforeTransition;
            _sceneLoadState.Progress.Progressed += OnTransitionProgressed;
            _sceneLoadState.OnTransitionOut += OnBeforeTransitionOut;
            
            _transitionView.SetLabelText("Loading...");
            _transitionView.DisableAsync(0f, .25f).Forget();
        }

        public void Tick()
        {
            if(!_isTransitionViewEnabled) return;
            
            _transitionModel.UpdateData();
            
            _transitionView.SetFillAmount(_transitionModel.FillAmount);

            var percentage = _transitionModel.FillAmount * 100f;
            
            _transitionView.SetPercentageText(percentage.ToString("0.0") + "%");
        }
        
        private void OnBeforeTransition()
        {
            _isTransitionViewEnabled = _sceneLoadState.IsTransitionViewActivated;
            
            if (!_isTransitionViewEnabled) return;

            _transitionModel.ResetProgress();
            
            _transitionView.SetFillAmount(_transitionModel.FillAmount);
            
            _transitionView.EnableAsync().Forget();
        }

        private void OnTransitionProgressed(float ratio) => _transitionModel.SetTargetRatio(ratio);
        private async UniTask OnBeforeTransitionOut()
        {
            if (!_isTransitionViewEnabled) return;

            while (!Mathf.Approximately(_transitionModel.FillAmount, 1f))
            {
                await UniTask.Yield();
            }

            await _transitionView.DisableAsync(delay: 0.25f);
            
            _isTransitionViewEnabled = false;
        }
        
        public void Dispose()
        {
            _sceneLoadState.OnLoadStart -= OnBeforeTransition;
            _sceneLoadState.Progress.Progressed -= OnTransitionProgressed;
            _sceneLoadState.OnTransitionOut -= OnBeforeTransitionOut;
        }
    }
}