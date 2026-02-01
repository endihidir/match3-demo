using System;
using Cysharp.Threading.Tasks;
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

        public async UniTask OpenSuccessMenuViewAsync()
        {
            await UniTask.WaitForSeconds(0.5f);
            
            SuccessMenuView?.SetActive(true);
        }

        public async UniTask OpenFailMenuViewAsync()
        {
            await UniTask.WaitForSeconds(0.5f);
            
            FailMenuView?.SetActive(true);
        }

        public void Dispose()
        {
            
        }
    }
}