using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class LevelEndView : MonoBehaviour, ILevelEndView, IDisposable
    {
        [field: SerializeField] private GameObject SuccessMenuView { get; set; }
        [field: SerializeField] private GameObject FailMenuView { get; set; }
        
        [field: SerializeField] private Button NextButton { get; set; }
        [field: SerializeField] private Button TryAgainButton { get; set; }
        
        public Button.ButtonClickedEvent OnClickNextButton => NextButton.onClick;
        public Button.ButtonClickedEvent OnClickTryAgainButton => TryAgainButton.onClick;
        
        public void Initialize()
        {
            SuccessMenuView?.SetActive(false);
            FailMenuView?.SetActive(false);
        }

        public void OpenSuccessMenuView() => SuccessMenuView?.SetActive(true);
        public void OpenFailMenuView() => FailMenuView?.SetActive(true);
        public void Dispose()
        {
            
        }
    }
}