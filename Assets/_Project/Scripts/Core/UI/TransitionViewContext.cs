using Core.Models;
using Core.Presenters;
using Core.Views;
using UnityEngine;

namespace Core.UI
{
    public class TransitionViewContext : BaseViewContext
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