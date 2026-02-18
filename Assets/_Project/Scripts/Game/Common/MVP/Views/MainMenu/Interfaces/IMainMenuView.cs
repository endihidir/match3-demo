using UnityEngine.UI;

namespace Game.Menu.Views
{
    public interface IMainMenuView
    {
        public Button.ButtonClickedEvent ClickedEvent { get; }
        void SetLevelNumber(int levelNumber);
        void EnableButton(bool value);
    }
}