using Core.Presenters;
using Core.Views;
using UnityEngine;

namespace Core.UI
{
    public class MainMenuContextView : BaseContextView
    {
        [field: SerializeField] private PlayButtonUI PlayButtonUI { get; set; }
        
        protected override void Initialize()
        {
            var mainMenuView = MVPContext.ResolveView<MainMenuView>();
            var playButtonView = MVPContext.ResolveView<PlayButtonView>().Initialize(PlayButtonUI.Button, PlayButtonUI.Label);
            
            MVPContext.ResolvePresenter<MainMenuPresenter>().Initialize(mainMenuView, playButtonView);
        }
    }
}