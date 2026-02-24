using Core.Models;
using Core.Presenters;
using Core.Views;
using VContainer;
using VContainer.Unity;

namespace Game.DI
{
    public class LoadingLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SceneTransitionPresenter>();
            builder.Register<SceneTransitionModel>(Lifetime.Scoped).As<ISceneTransitionModel>();
            builder.RegisterComponentInHierarchy<SceneTransitionView>().As<ISceneTransitionView>();
        }
    }
}