using Core.Bootstrapper;
using Core.Presenters;
using Core.Views;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class MenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MenuBootstrapper>();
            
            builder.RegisterComponentInHierarchy<PlayButtonView>().As<IPlayButtonView>();
            builder.Register<MainMenuPresenter>(Lifetime.Scoped).As<IInitializable>();
        }

        private void Start() => Build();
    }
}