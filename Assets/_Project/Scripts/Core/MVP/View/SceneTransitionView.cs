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
        UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null);
        UniTask DisableAsync(float duration = 0.2f, float delay = 0f, Action onComplete = null);
        void SetFillAmount(float value);
        void SetLabelText(string value);
        void SetPercentageText(string value);
    }
    
    public class SceneTransitionView : ISceneTransitionView
    {
        private GameObject[] _toggleObjects;
        private Image _sliderImage;
        private TextMeshProUGUI _sliderTxt, _percentageTxt;
        private IFadeAnimationView _fadeAnimationView;
        
        public ISceneTransitionView Initialize(IFadeAnimationView fadeAnimationView, ProgressBarUI progressBarUI, GameObject[] toggleObjects)
        {
            _sliderImage = progressBarUI.SliderImage;
            _sliderTxt = progressBarUI.SliderTxt;
            _percentageTxt = progressBarUI.PercentageTxt;
            _toggleObjects = toggleObjects;
            _fadeAnimationView = fadeAnimationView;
            return this;
        }

        public async UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            SetActiveToggleObjects(true);

            await _fadeAnimationView.FadeInAsync(duration, delay, onComplete);
        }

        public async UniTask DisableAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null)
        {
            await _fadeAnimationView.FadeOutAsync(duration, delay, onComplete);
            
            SetActiveToggleObjects(false);
        }
        
        public void SetFillAmount(float value)
        {
            if(!_sliderImage) return;
            
            _sliderImage.fillAmount = value;
        }

        public void SetPercentageText(string value) => _percentageTxt?.SetText(value);
        public void SetLabelText(string value) => _sliderTxt?.SetText(value);
        private void SetActiveToggleObjects(bool value) => Array.ForEach(_toggleObjects, sceneSystem => sceneSystem.SetActive(value));
    }
}