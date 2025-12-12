using System;
using Core.Models;
using Core.SceneService;
using Core.Views;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Core.Presenters
{
    public interface ISceneTransitionPresenter
    {
        ISceneTransitionPresenter Initialize(ISceneTransitionView sceneTransitionView);
    }
    public class SceneTransitionPresenter : ISceneTransitionPresenter, ITickable, IDisposable
    {
        private readonly ISceneLoadEvents _sceneLoadEvents;
        private readonly ISceneTransitionModel _transitionModel;
        private ISceneTransitionView _transitionView;
        private bool _isTransitionViewEnabled = false;
        public SceneTransitionPresenter(ISceneLoadEvents sceneLoadEvents, ISceneTransitionModel sceneTransitionTransitionModel)
        {
            _sceneLoadEvents = sceneLoadEvents;
            _transitionModel = sceneTransitionTransitionModel;
        }

        public ISceneTransitionPresenter Initialize(ISceneTransitionView sceneTransitionView)
        {
            _transitionView = sceneTransitionView;
            
            _sceneLoadEvents.OnBeforeTransition += OnBeforeTransition;
            _sceneLoadEvents.Progress.Progressed += OnTransitionProgressed;
            _sceneLoadEvents.OnBeforeTransitionOut += OnBeforeTransitionOut;
            
            _transitionView.SetLabelText("Loading...");
            _transitionView.DisableAsync(0f, .25f).Forget();
            return this;
        }

        public void Tick()
        {
            if(!_isTransitionViewEnabled) return;
            
            _transitionModel.UpdateData();
            
            _transitionView.SetFillAmount(_transitionModel.FillAmount);

            var percentage = _transitionModel.FillAmount * 100f;
            
            _transitionView.SetPercentageText(percentage.ToString("0.0") + "%");
        }
        
        private void OnBeforeTransition(bool useTransitionView)
        {
            _isTransitionViewEnabled = useTransitionView;
   
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
            _sceneLoadEvents.OnBeforeTransition -= OnBeforeTransition;
            _sceneLoadEvents.Progress.Progressed -= OnTransitionProgressed;
            _sceneLoadEvents.OnBeforeTransitionOut -= OnBeforeTransitionOut;
        }
    }
}