using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Views
{
    public class SceneTransitionView : MonoBehaviour, ISceneTransitionView
    {
        [field : SerializeField] private ProgressBarView ProgressBarView { get; set; }
        [field : SerializeField] private FadeAnimationView FadeAnimationView { get; set; }
        [field : SerializeField] private GameObject[] ToggleObjects { get; set; }

        public async UniTask EnableAsync(float duration = 0f, float delay = 0f, Action onComplete = null)
        {
            SetActiveToggleObjects(true);

            await FadeAnimationView.FadeInAsync(1f, duration, delay, onComplete);
        }

        public async UniTask DisableAsync(float duration = 0.2f, float delay = 1f, Action onComplete = null)
        {
            await FadeAnimationView.FadeOutAsync(0f, duration, delay, onComplete);
            
            SetActiveToggleObjects(false);
        }
        
        public void SetFillAmount(float value) => ProgressBarView?.SetFillAmount(value);
        public void SetPercentageText(string value) => ProgressBarView?.SetPercentageText(value);
        public void SetLabelText(string value) => ProgressBarView?.SetLabelText(value);
        private void SetActiveToggleObjects(bool value) => Array.ForEach(ToggleObjects, sceneSystem => sceneSystem.SetActive(value));
    }
}