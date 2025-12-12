using Core.Presenters;
using Core.Views;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public class MainMenuViewContext : SelfViewContext
    {
        [field: SerializeField] private PlayButtonView PlayButtonView { get; set; }
        
        protected override void Initialize()
        {
            ObjectResolver.Resolve<IMainMenuPresenter>()
                          .Initialize(PlayButtonView);
        }
    }
}