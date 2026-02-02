using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class LevelEndView : MonoBehaviour, ILevelEndView
    {
        [field: SerializeField] private LevelEndMenuAnimationView SuccessMenuView { get; set; }
        [field: SerializeField] private LevelEndMenuAnimationView FailMenuView { get; set; }
        
        [field: SerializeField] private Button NextButton { get; set; }
        [field: SerializeField] private Button TryAgainButton { get; set; }
        
        public Button.ButtonClickedEvent OnClickNextButton => NextButton.onClick;
        public Button.ButtonClickedEvent OnClickTryAgainButton => TryAgainButton.onClick;
        
        public void Initialize()
        {
            CloseFailMenu();
            CloseSuccessMenu();
        }

        public async UniTask OpenSuccessMenuViewAsync() => await SuccessMenuView.ShowAsync(.5f);
        public async UniTask OpenFailMenuViewAsync() => await FailMenuView.ShowAsync(.5f);
        public void CloseSuccessMenu() => SuccessMenuView.Hide();
        public void CloseFailMenu() => FailMenuView.Hide();
    }
}