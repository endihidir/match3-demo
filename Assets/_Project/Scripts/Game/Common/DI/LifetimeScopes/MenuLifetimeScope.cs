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
            builder.Register<PlayButtonView>(Lifetime.Scoped).As<IPlayButtonView>();
            builder.Register<MainMenuPresenter>(Lifetime.Scoped).As<IMainMenuPresenter>();
        }
    }
}