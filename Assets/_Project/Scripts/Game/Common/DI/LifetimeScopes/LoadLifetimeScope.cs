using System;
using Core.Bootstrapper;
using Core.Context;
using Core.Models;
using Core.Presenters;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class LoadLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<LoadingBootstrapper>();
            
            builder.RegisterComponentInHierarchy<LoadingViewContext>().As<ILoadingViewContext>();
            builder.Register<SceneTransitionModel>(Lifetime.Scoped).As<ISceneTransitionModel>();
            builder.Register<SceneTransitionPresenter>(Lifetime.Scoped).As<ISceneTransitionPresenter, ITickable>();
        }

        private void Start() => Build();
    }
}