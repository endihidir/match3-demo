using Core.Presenters;
using Core.Views;
using UnityEngine;

namespace Core.UI
{
    public class MenuSceneViewContext : BaseViewContext
    {
        [field: SerializeField] private PlayButtonUI PlayButtonUI { get; set; }
        
        protected override void Initialize()
        {
            var playButtonView = MVPContext.ResolveView<PlayButtonView>()
                                           .Initialize(PlayButtonUI);
            
            MVPContext.ResolvePresenter<MainMenuPresenter>()
                      .Initialize(playButtonView);
        }
    }
}