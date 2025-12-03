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
            var transitionModel = MVPContext.ResolveModel<SceneTransitionModel>();
            
            var fadeAnimation = MVPContext.ResolveView<FadeAnimationView>()
                                          .Initialize(CanvasGroup);

            var transitionView = MVPContext.ResolveView<SceneTransitionView>()
                                           .Initialize(fadeAnimation, ProgressBarUI, ToggleObjects);
            
            MVPContext.ResolvePresenter<SceneTransitionPresenter>()
                      .Initialize(transitionModel, transitionView);
        }
    }
}