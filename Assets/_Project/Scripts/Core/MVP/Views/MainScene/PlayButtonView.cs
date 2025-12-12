using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ButtonClickedEvent = UnityEngine.UI.Button.ButtonClickedEvent;

namespace Core.Views
{
    public interface IPlayButtonView
    {
        public ButtonClickedEvent ClickedEvent { get; }
        void SetText(string text);
        void EnableButton(bool value);
    }
    public class PlayButtonView : MonoBehaviour, IPlayButtonView
    {
        [field: SerializeField] private Button Button { get; set; }
        [field: SerializeField] private TextMeshProUGUI Label { get; set; }

        public ButtonClickedEvent ClickedEvent => Button.onClick;
        
        public void EnableButton(bool value) => Button.interactable = value;
        public void SetText(string text) => Label?.SetText(text);
    }
}
