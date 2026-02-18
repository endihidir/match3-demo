using Cysharp.Threading.Tasks;
using UnityEngine.UI;

namespace Game.Views
{
    public interface ILevelEndView
    {
        Button.ButtonClickedEvent OnClickNextButton { get; }
        Button.ButtonClickedEvent OnClickTryAgainButton { get; }
        void Initialize();
        UniTask OpenSuccessMenuViewAsync();
        UniTask OpenFailMenuViewAsync();
        public void CloseSuccessMenu();
        public void CloseFailMenu();
    }
}