using UnityEngine;

namespace Core.Views
{
    public interface IMainMenuViewContext
    {
        PlayButtonView PlayButtonView { get; }
    }
    
    public class MainMenuViewContext : MonoBehaviour, IMainMenuViewContext
    {
        [field: SerializeField] public PlayButtonView PlayButtonView { get; private set; }
    }
}