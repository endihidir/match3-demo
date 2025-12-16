using Core.Bootstrapper;
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
            builder.RegisterEntryPoint<LoadingBootstrapper>();
            
            builder.Register<SceneTransitionModel>(Lifetime.Scoped).As<ISceneTransitionModel>();
            builder.RegisterComponentInHierarchy<SceneTransitionView>().As<ISceneTransitionView>();
            builder.Register<SceneTransitionPresenter>(Lifetime.Scoped).As<IInitializable, ITickable>();
        }

        private void Start() => Build();
    }
}