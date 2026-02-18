using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ButtonClickedEvent = UnityEngine.UI.Button.ButtonClickedEvent;

namespace Game.Menu.Views
{
    public class MainMenuView : MonoBehaviour, IMainMenuView
    {
        [field: SerializeField] private Button Button { get; set; }
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }

        public ButtonClickedEvent ClickedEvent => Button.onClick;
        
        public void EnableButton(bool value) => Button.interactable = value;
        public void SetLevelNumber(int levelNumber) => Label?.SetText($"Level {levelNumber}");
    }
}
