using Core.Bootstrapper;
using Core.Configs;
using Core.Handlers;
using Core.Item.Factories;
using Core.Models;
using Core.Presenters;
using Core.Services;
using Core.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private GameplayConfigContainer GameplayConfigContainer {get; set;}

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(GameplayConfigContainer);
            builder.RegisterEntryPoint<GameplayBootstrapper>();
            
            builder.Register<InputService>(Lifetime.Scoped).As<IInputService, ITickable>();
            builder.Register<LevelGoalModel>(Lifetime.Scoped).As<ILevelGoalModel>();
            
            builder.Register<GridItemFactory>(Lifetime.Scoped).As<IGridItemFactory>();
            builder.Register<GridItemFactoryHandler>(Lifetime.Scoped).As<IGridItemFactoryHandler>();
            
            builder.Register<GridStateHandler>(Lifetime.Scoped).As<IGridStateHandler, ITickable, IFixedTickable, ILateTickable>();
            builder.Register<GridModel>(Lifetime.Scoped).As<IGridModel>();
            builder.RegisterComponentInHierarchy<GridView>().As<IGridView>();
            builder.Register<GridPresenter>(Lifetime.Scoped).As<IInitializable>();
            builder.Register<GridCheatHandler>(Lifetime.Scoped).As<IGridCheatHandler, ITickable>();
            
            builder.Register<FillStrategyHandler>(Lifetime.Scoped).As<IFillStrategyHandler>();
            builder.Register<FallDownFillStrategy>(Lifetime.Scoped).As<IFillStrategy>();
            builder.Register<SlideDownFillStrategy>(Lifetime.Scoped).As<IFillStrategy>();
        }

        private void Start()
        {
            Build();
        }
    }
}