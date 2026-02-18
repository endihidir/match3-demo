using Game.Bootstrappers;
using Game.Configs;
using Game.Grid.Item.Factories;
using Game.Grid.Strategies;
using Game.Grid.Strategies.Schedulers;
using Game.Grid.Handlers;
using Game.Grid.Services;
using Game.HUD.Handlers;
using Game.Level.Models;
using Game.Models;
using Game.Presenters;
using Game.Services;
using Game.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private GameplayConfigContainer GameplayConfigContainer { get; set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(GameplayConfigContainer.GridConfigContainer);
            builder.RegisterEntryPoint<GameplayBootstrapper>();
    
            // SERVICE
            builder.Register<GameplaySetupService>(Lifetime.Scoped).As<IGameplaySetupService>();
            builder.Register<GridInputService>(Lifetime.Scoped).As<IGridInputService, ITickable>();
    
            // LEVEL
            builder.Register<LevelObjectiveModel>(Lifetime.Scoped).As<ILevelObjectiveModel>();
            builder.RegisterComponentInHierarchy<LevelEndView>().As<ILevelEndView>();
            builder.RegisterEntryPoint<LevelEndPresenter>();
    
            // HUD
            builder.Register<GoalSlotHandler>(Lifetime.Scoped).As<IGoalSlotHandler>();
            builder.Register<GoalFxHandler>(Lifetime.Scoped).As<IGoalFxHandler>();
            
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterComponentInHierarchy<HudView>().As<IHudView>();
    
            // GRID
            builder.Register<GridObjectFactory>(Lifetime.Scoped).As<IGridObjectFactory>();
            
            builder.Register<GridObjectHandler>(Lifetime.Scoped).As<IGridObjectHandler>();
            builder.Register<BlastFxHandler>(Lifetime.Scoped).As<IBlastFxHandler>();
            builder.Register<BoosterFxHandler>(Lifetime.Scoped).As<IBoosterFxHandler>();
            
            builder.Register<GridModel>(Lifetime.Scoped).As<IGridModel>();
            builder.RegisterComponentInHierarchy<GridView>().As<IGridView>();
            builder.RegisterEntryPoint<GridPresenter>();
            builder.Register<GridCheatHandler>(Lifetime.Scoped).As<IGridCheatHandler, ITickable>();
            builder.Register<GridStateHandler>(Lifetime.Scoped).As<IGridStateHandler, ITickable, IFixedTickable, ILateTickable>();
    
            // GRID FILL
            builder.Register<FillStrategyResolver>(Lifetime.Scoped).As<IFillStrategyResolver>();
            builder.Register<SlideDownFillStrategy>(Lifetime.Scoped).As<IFillStrategy>();
            builder.Register<FallDownFillStrategy>(Lifetime.Scoped).As<IFillStrategy>();
            builder.Register<SlideAnimationScheduler>(Lifetime.Scoped).As<ISlideAnimationScheduler>();
            builder.Register<FallAnimationScheduler>(Lifetime.Scoped).As<IFallAnimationScheduler>();
            builder.Register<FillItemDecider>(Lifetime.Scoped).As<IFillItemDecider>();
        }
    }
}