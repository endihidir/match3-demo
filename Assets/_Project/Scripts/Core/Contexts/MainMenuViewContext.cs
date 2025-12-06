using Core.Presenters;
using Core.UI;
using Core.Views;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public class MainMenuViewContext : SelfViewContext
    {
        [field: SerializeField] private PlayButtonUI PlayButtonUI { get; set; }
        
        protected override void Initialize()
        {
            var playButtonView = ObjectResolver.Resolve<IPlayButtonView>().Initialize(PlayButtonUI);
            
            ObjectResolver.Resolve<IMainMenuPresenter>()
                          .Initialize(playButtonView);
        }
    }
}