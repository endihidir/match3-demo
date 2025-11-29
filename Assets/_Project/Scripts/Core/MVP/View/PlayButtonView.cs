using Core.MVPContext.Interfaces;
using TMPro;
using UnityEngine.UI;

namespace Core.Views
{
    public interface IPlayButtonView : IView
    {
        public Button Button { get; }
        void SetText(string text);
        IPlayButtonView Initialize(Button button, TextMeshProUGUI buttonText);
    }
    public sealed class PlayButtonView : IPlayButtonView
    {
        public Button Button { get; private set; }
        private TextMeshProUGUI _label;
        
        public IPlayButtonView Initialize(Button button, TextMeshProUGUI label)
        {
            Button  = button;
            _label = label;
            return this;
        }

        public void SetText(string text) => _label?.SetText(text);
    }
}