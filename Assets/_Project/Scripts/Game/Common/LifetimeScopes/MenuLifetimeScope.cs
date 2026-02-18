using Game.Menu.Views;
using Game.Presenters;
using VContainer;
using VContainer.Unity;

namespace Game.DI
{
    public class MenuLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MainMenuPresenter>();
            builder.RegisterComponentInHierarchy<MainMenuView>().As<IMainMenuView>();
        }
    }
}