using Core.Models;
using Core.Presenters;
using Core.Views;
using UnityEngine;

namespace Core.UI
{
    public class BootSceneViewContext : BaseViewContext
    {
        [field: SerializeField] private GameObject[] SceneSystems { get; set; }
        [field: SerializeField] private CanvasGroup CanvasGroup { get; set; }
        [field: SerializeField] private ProgressBarUI ProgressBarUI { get; set; }
        
        protected override void Initialize()
        {
            var sceneTransitionProgressModel = MVPContext.ResolveModel<SceneTransitionProgressModel>();
            
            var fadeAnimationView = MVPContext.ResolveView<FadeAnimationView>()
                                              .Initialize(CanvasGroup);

            var sceneTransitionView = MVPContext.ResolveView<SceneTransitionView>()
                                                .Initialize(CanvasGroup, ProgressBarUI, SceneSystems, fadeAnimationView);
            
            MVPContext.ResolvePresenter<SceneTransitionPresenter>()
                      .Initialize(sceneTransitionProgressModel,  sceneTransitionView);
        }
    }
}