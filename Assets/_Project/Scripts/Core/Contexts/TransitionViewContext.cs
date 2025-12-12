using Core.Models;
using Core.Presenters;
using Core.Views;
using UnityEngine;
using VContainer;

namespace Core.Context
{
    public class TransitionViewContext : SelfViewContext
    {
        [field: SerializeField] private SceneTransitionView SceneTransitionView { get; set; }
        
        protected override void Initialize()
        {
            var transitionModel = ObjectResolver.Resolve<ISceneTransitionModel>();
            
            ObjectResolver.Resolve<ISceneTransitionPresenter>()
                          .Initialize(transitionModel, SceneTransitionView);
        }
    }
}