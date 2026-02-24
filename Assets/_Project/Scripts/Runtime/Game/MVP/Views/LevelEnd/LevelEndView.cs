using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Views
{
    public class LevelEndView : MonoBehaviour, ILevelEndView
    {
        [field: SerializeField] private AnimatedPanelModule SuccessPanel { get; set; }
        [field: SerializeField] private AnimatedPanelModule FailPanel { get; set; }
        
        [field: SerializeField] private Button NextButton { get; set; }
        [field: SerializeField] private Button TryAgainButton { get; set; }
        
        public Button.ButtonClickedEvent OnClickNextButton => NextButton.onClick;
        public Button.ButtonClickedEvent OnClickTryAgainButton => TryAgainButton.onClick;
        
        public void Initialize()
        {
            CloseFailMenu();
            CloseSuccessMenu();
        }

        public async UniTask OpenSuccessMenuViewAsync() => await SuccessPanel.ShowAsync(.5f);
        public async UniTask OpenFailMenuViewAsync() => await FailPanel.ShowAsync(.5f);
        public void CloseSuccessMenu() => SuccessPanel.Hide();
        public void CloseFailMenu() => FailPanel.Hide();
    }
}