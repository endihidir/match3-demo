using Core.Models;
using Core.Presenters;
using Core.UI;
using Core.Views;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public class TransitionScreenContext : SelfViewContext
    {
        [field: SerializeField] private GameObject[] ToggleObjects { get; set; }
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private ProgressBarUI ProgressBarUI { get; set; }
        
        protected override void Initialize()
        {
            var transitionModel = ObjectResolver.Resolve<ISceneTransitionModel>();
            
            var fadeAnimation = ObjectResolver.Resolve<IFadeAnimationView>()
                                              .Initialize(CanvasGroup);

            var transitionView = ObjectResolver.Resolve<ISceneTransitionView>()
                                               .Initialize(fadeAnimation, ProgressBarUI, ToggleObjects);
            
            ObjectResolver.Resolve<ISceneTransitionPresenter>()
                          .Initialize(transitionModel, transitionView);
        }
    }
}