using UnityEngine.UI;

namespace Core.UI
{
    public interface ILevelEndView
    {
        Button.ButtonClickedEvent OnClickNextButton { get; }
        Button.ButtonClickedEvent OnClickTryAgainButton { get; }
        void Initialize();
        void OpenSuccessMenuView();
        void OpenFailMenuView();
    }
}