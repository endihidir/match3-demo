using Core.MVPContext.Interfaces;
using Core.UI;
using TMPro;
using UnityEngine.UI;
using ButtonClickedEvent = UnityEngine.UI.Button.ButtonClickedEvent;

namespace Core.Views
{
    public interface IPlayButtonView : IView
    {
        IPlayButtonView Initialize(PlayButtonUI playButtonUI);
        public ButtonClickedEvent ClickedEvent { get; }
        void SetText(string text);
        void EnableButton(bool value);
    }
    public sealed class PlayButtonView : IPlayButtonView
    {
        private Button _playButton;
        private TextMeshProUGUI _label;
        public ButtonClickedEvent ClickedEvent { get; private set; }
        
        public IPlayButtonView Initialize(PlayButtonUI playButtonUI)
        {
            _playButton = playButtonUI.Button;
            _label = playButtonUI.Label;
            ClickedEvent = _playButton.onClick;
            return this;
        }

        public void EnableButton(bool value) => _playButton.interactable = value;
        public void SetText(string text) => _label?.SetText(text);
    }
}