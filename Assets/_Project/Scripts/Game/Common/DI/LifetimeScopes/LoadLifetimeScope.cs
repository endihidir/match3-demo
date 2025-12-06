using Core.Models;
using Core.Presenters;
using Core.Views;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class LoadLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneTransitionModel>(Lifetime.Scoped).As<ISceneTransitionModel>();
            builder.Register<FadeAnimationView>(Lifetime.Scoped).As<IFadeAnimationView>();
            builder.Register<SceneTransitionView>(Lifetime.Scoped).As<ISceneTransitionView>();
            builder.Register<SceneTransitionPresenter>(Lifetime.Scoped).As<ISceneTransitionPresenter, ITickable>();
        }
    }
}