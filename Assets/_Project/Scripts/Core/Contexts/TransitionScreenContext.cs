using Core.Models;
using Core.Presenters;
using Core.UI;
using Core.Views;
using UnityEngine;

namespace Core.Context
{
    public class TransitionScreenContext : SelfViewContext
    {
        [field: SerializeField] private GameObject[] ToggleObjects { get; set; }
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private ProgressBarUI ProgressBarUI { get; set; }
        
        protected override void Initialize()
        {
            var transitionModel = OwnerContext.ResolveModel<SceneTransitionModel>();
            
            var fadeAnimation = OwnerContext.ResolveView<FadeAnimationView>()
                                          .Initialize(CanvasGroup);

            var transitionView = OwnerContext.ResolveView<SceneTransitionView>()
                                           .Initialize(fadeAnimation, ProgressBarUI, ToggleObjects);
            
            OwnerContext.ResolvePresenter<SceneTransitionPresenter>()
                      .Initialize(transitionModel, transitionView);
        }
    }
}