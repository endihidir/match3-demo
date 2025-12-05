using Core.Presenters;
using Core.UI;
using Core.Views;
using UnityEngine;

namespace Core.Context
{
    public class MainMenuViewContext : SelfViewContext
    {
        [field: SerializeField] private PlayButtonUI PlayButtonUI { get; set; }
        
        protected override void Initialize()
        {
            var playButtonView = OwnerContext.ResolveView<PlayButtonView>()
                                             .Initialize(PlayButtonUI);
            
            OwnerContext.ResolvePresenter<MainMenuPresenter>()
                        .Initialize(playButtonView);
        }
    }
}