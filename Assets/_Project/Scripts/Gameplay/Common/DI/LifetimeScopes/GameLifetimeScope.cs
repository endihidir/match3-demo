using Core.Bootstrapper;
using Core.Configs;
using Core.SceneService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfigContainer _gameConfigContainer;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameConfigContainer);

            builder.RegisterEntryPoint<GameBootstrapper>();

            builder.Register<GridPresenter>(Lifetime.Scoped).As<IGridPresenter>();
        }
    }
}