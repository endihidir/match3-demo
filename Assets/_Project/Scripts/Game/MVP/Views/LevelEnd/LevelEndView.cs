using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class LevelEndView : MonoBehaviour, ILevelEndView
    {
        [field: SerializeField] private AnimatedPanelModule SuccessMenuModule { get; set; }
        [field: SerializeField] private AnimatedPanelModule FailMenuModule { get; set; }
        
        [field: SerializeField] private Button NextButton { get; set; }
        [field: SerializeField] private Button TryAgainButton { get; set; }
        
        public Button.ButtonClickedEvent OnClickNextButton => NextButton.onClick;
        public Button.ButtonClickedEvent OnClickTryAgainButton => TryAgainButton.onClick;
        
        public void Initialize()
        {
            CloseFailMenu();
            CloseSuccessMenu();
        }

        public async UniTask OpenSuccessMenuViewAsync() => await SuccessMenuModule.ShowAsync(.5f);
        public async UniTask OpenFailMenuViewAsync() => await FailMenuModule.ShowAsync(.5f);
        public void CloseSuccessMenu() => SuccessMenuModule.Hide();
        public void CloseFailMenu() => FailMenuModule.Hide();
    }
}