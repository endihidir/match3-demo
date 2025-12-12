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
            builder.Register<SceneTransitionModel>(Lifetime.Scoped).As<ISceneTransitionModel>();
            builder.Register<SceneTransitionPresenter>(Lifetime.Scoped).As<ISceneTransitionPresenter, ITickable>();
        }
    }
}