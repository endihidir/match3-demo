using System;
using Core.MVPContext.Interfaces;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Views
{
    public interface ISceneTransitionView : IView
    {
        bool IsActive { get; }
        UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null);
        UniTask DisableAsync(float duration = 0.2f, float delay = 0f, Action onComplete = null);
        void SetFillAmount(float value);
        void SetLabelText(string value);
        void SetPercentageText(string value);
    }
    
    public class SceneTransitionView : ISceneTransitionView
    {
        private GameObject[] _sceneSystems;
        private CanvasGroup _menuCanvasGroup;
        private Image _sliderImage;
        private TextMeshProUGUI _sliderTxt, _percentageTxt;
        private IFadeAnimationView _fadeAnimationView;
        public bool IsActive => _menuCanvasGroup.interactable;
        
        public ISceneTransitionView Initialize(CanvasGroup loadingCanvasGroup, ProgressBarUI progressBarUI, GameObject[] sceneSystems, IFadeAnimationView fadeAnimationView)
        {
            _menuCanvasGroup = loadingCanvasGroup;
            _sliderImage = progressBarUI.SliderImage;
            _sliderTxt = progressBarUI.SliderTxt;
            _percentageTxt = progressBarUI.PercentageTxt;
            _sceneSystems = sceneSystems;
            _fadeAnimationView = fadeAnimationView;
            return this;
        }

        public async UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            SetActiveSceneSystems(true);
            
            SetActiveInteractable(true);

            await _fadeAnimationView.EnableAsync(duration, delay, onComplete);
        }

        public async UniTask DisableAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null)
        {
            SetActiveInteractable(false);

            await _fadeAnimationView.DisableAsync(duration, delay, onComplete);
            
            SetActiveSceneSystems(false);
        }

        private void SetActiveInteractable(bool value)
        {
            if(!_menuCanvasGroup) return;
            _menuCanvasGroup.interactable = value;
            _menuCanvasGroup.blocksRaycasts = value;
        }

        private void SetActiveSceneSystems(bool value) => Array.ForEach(_sceneSystems, sceneSystem => sceneSystem.SetActive(value));

        public void SetFillAmount(float value)
        {
            if(!_sliderImage) return;
            _sliderImage.fillAmount = value;
        }

        public void SetLabelText(string value) => _sliderTxt?.SetText(value);
        public void SetPercentageText(string value) => _percentageTxt?.SetText(value);
    }
}