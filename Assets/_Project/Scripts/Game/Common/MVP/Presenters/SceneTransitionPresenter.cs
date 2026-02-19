using System;
using Core.Models;
using Core.Scene.Services;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
{
    public sealed class SceneTransitionPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly ISceneLoadState _loadState;
        private readonly ISceneTransitionModel _transitionModel;
        private readonly ISceneTransitionView _transitionView;
        private bool _isTransitionViewEnabled = false;
        public SceneTransitionPresenter(ISceneLoadState loadState, ISceneTransitionModel transitionModel, ISceneTransitionView transitionView)
        {
            _loadState = loadState;
            _transitionModel = transitionModel;
            _transitionView = transitionView;
        }

        public void Initialize()
        {
            _loadState.OnLoadStart += OnTransitionStart;
            _loadState.Progress.Progressed += OnTransitionProgress;
            _loadState.OnTransitionOut += OnTransitionOut;
            
            _transitionView.SetLabelText("Loading...");
            _transitionView.DisableAsync(0f, .25f).Forget();
        }
        
        private void OnTransitionStart()
        {
            _isTransitionViewEnabled = _loadState.IsTransitionViewActivated;
            if (!_isTransitionViewEnabled) return;

            _transitionModel.ResetProgress();
            _transitionView.SetFillAmount(_transitionModel.FillAmount);
            _transitionView.EnableAsync().Forget();
        }

        private void OnTransitionProgress(float ratio) => _transitionModel.SetTargetRatio(ratio);
        private async UniTask OnTransitionOut()
        {
            if (!_isTransitionViewEnabled) return;

            while (!Mathf.Approximately(_transitionModel.FillAmount, 1f))
            {
                await UniTask.Yield();
            }

            await _transitionView.DisableAsync(delay: 0.25f);
            
            _isTransitionViewEnabled = false;
        }
      
        public void Tick()
        {
            if(!_isTransitionViewEnabled) return;
            
            _transitionModel.UpdateData();
            _transitionView.SetFillAmount(_transitionModel.FillAmount);

            var ratio = _transitionModel.FillAmount * 100f;
            _transitionView.SetPercentageText(ratio.ToString("0.0") + "%");
        }
        
        public void Dispose()
        {
            _loadState.OnLoadStart -= OnTransitionStart;
            _loadState.Progress.Progressed -= OnTransitionProgress;
            _loadState.OnTransitionOut -= OnTransitionOut;
        }
    }
}