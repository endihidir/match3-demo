using Core.Bootstrapper;
using Core.Configs;
using Core.Handlers;
using Core.Item.Factories;
using Core.Models;
using Core.Presenters;
using Core.Services;
using Core.Views;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [field: FormerlySerializedAs("<GameConfigContainer>k__BackingField")] [field: SerializeField] private GameplayConfigContainer GameplayConfigContainer {get; set;}

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

            builder.Register<MatchResolveHandler>(Lifetime.Scoped).As<IMatchResolveHandler>();
            builder.Register<BoosterSelectionHandler>(Lifetime.Scoped).As<IBoosterSelectionHandler>();
            
            builder.Register<RefillStrategyHandler>(Lifetime.Scoped).As<IRefillStrategyHandler>();
            builder.Register<FallDownRefillStrategy>(Lifetime.Scoped).As<IRefillStrategy>();
            builder.Register<SlideDownRefillStrategy>(Lifetime.Scoped).As<IRefillStrategy>();
        }

        private void Start()
        {
            Build();
        }
    }
}