using Core.Bootstrapper;
using Core.Configs;
using Core.Item.Factories;
using Core.SceneService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private GameConfigContainer GameConfigContainer {get; set;}

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(GameConfigContainer);

            builder.RegisterEntryPoint<GameBootstrapper>();

            builder.Register<GridPresenter>(Lifetime.Scoped).As<IGridPresenter>();
            
            builder.Register<GridItemFactory>(Lifetime.Scoped).As<IGridItemFactory>();
            
            builder.Register<ItemEffectFactory>(Lifetime.Scoped).As<IItemEffectFactory>();
        }
    }
}