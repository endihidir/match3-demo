using System;
using Core.Models;
using Core.MVPContext.Interfaces;
using Core.SceneService;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Presenters
{
    public interface ISceneTransitionPresenter : IPresenter, IUpdater, IDisposable
    {
        public void StartLoadingProgress(float ratio);
        public void ResetProgress();
    }
    
    public class SceneTransitionPresenter : ISceneTransitionPresenter
    {
        private readonly ISceneLoadEvents _sceneLoadEvents;
        private ISceneTransitionProgressModel _model;
        private ISceneTransitionView _view;
        private bool _isTransitionViewEnabled = false;

        public SceneTransitionPresenter(ISceneLoadEvents sceneLoadEvents) => _sceneLoadEvents = sceneLoadEvents;

        public ISceneTransitionPresenter Initialize(ISceneTransitionProgressModel progressModel, ISceneTransitionView view)
        {
            _model = progressModel;
            _view = view;
            
            _sceneLoadEvents.OnBeforeTransition += OnBeforeTransition;
            _sceneLoadEvents.Progress.Progressed += OnTransitionProgressed;
            _sceneLoadEvents.OnBeforeTransitionOut += OnBeforeTransitionOut;
            
            _view.SetLabelText("Loading...");
            _view.DisableAsync(0f, .25f).Forget();
            return this;
        }
        
        public void StartLoadingProgress(float ratio) => _model.SetTargetRatio(ratio);
        public void ResetProgress() => _model.ResetProgress();

        public void Update()
        {
            if(!_isTransitionViewEnabled) return;
            
            _model.UpdateData();
            
            _view.SetFillAmount(_model.FillAmount);

            var percentage = _model.FillAmount * 100f;
            
            _view.SetPercentageText(percentage.ToString("0.0") + "%");
        }
        
        private void OnBeforeTransition(bool useTransitionView)
        {
            _isTransitionViewEnabled = useTransitionView;
   
            if (!_isTransitionViewEnabled) return;
            
            ResetProgress();
            
            _view.EnableAsync().Forget();
        }

        private void OnTransitionProgressed(float ratio) => StartLoadingProgress(ratio);
        private async UniTask OnBeforeTransitionOut()
        {
            if (!_isTransitionViewEnabled) return;

            while (!Mathf.Approximately(_model.FillAmount, 1f))
            {
                await UniTask.Yield();
            }

            await _view.DisableAsync(delay: 0.25f);
            
            _isTransitionViewEnabled = false;
        }
        
        public void Dispose()
        {
            _sceneLoadEvents.OnBeforeTransition -= OnBeforeTransition;
            _sceneLoadEvents.Progress.Progressed -= OnTransitionProgressed;
            _sceneLoadEvents.OnBeforeTransitionOut -= OnBeforeTransitionOut;
        }
    }
}