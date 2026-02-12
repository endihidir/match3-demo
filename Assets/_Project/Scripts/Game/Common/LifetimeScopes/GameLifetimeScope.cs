using Core.Bootstrapper;
using Core.Configs;
using Core.Handlers;
using Core.Item.Factories;
using Core.Models;
using Core.Presenters;
using Core.Services;
using Core.UI;
using Core.Views;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class GameLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private GameplayConfigContainer GameplayConfigContainer { get; set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(GameplayConfigContainer);
            builder.RegisterEntryPoint<GameSceneSessionController>();
    
            // SERVICE
            builder.Register<GameplaySetupService>(Lifetime.Scoped).As<IGameplaySetupService>();
            builder.Register<GridInputService>(Lifetime.Scoped).As<IGridInputService, ITickable>();
    
            // LEVEL
            builder.Register<LevelObjectiveModel>(Lifetime.Scoped).As<ILevelObjectiveModel>();
            builder.RegisterComponentInHierarchy<LevelEndView>().As<ILevelEndView>();
            builder.RegisterEntryPoint<LevelEndPresenter>();
    
            // HUD
            builder.RegisterEntryPoint<HudPresenter>();
            builder.RegisterComponentInHierarchy<HudView>().As<IHudView>();
            builder.Register<GoalSlotFactory>(Lifetime.Scoped).As<IGoalSlotFactory>();
            builder.Register<GoalFxHandler>(Lifetime.Scoped).As<IGoalFxHandler>();
    
            // GRID
            builder.Register<BlastFxFactory>(Lifetime.Scoped).As<IBlastFxFactory>();
            builder.Register<BlastFxHandler>(Lifetime.Scoped).As<IBlastFxHandler>();
            builder.Register<BoosterFxFactory>(Lifetime.Scoped).As<IBoosterFxFactory>();
            builder.Register<BoosterFxHandler>(Lifetime.Scoped).As<IBoosterFxHandler>();
            
            builder.Register<GridItemFactory>(Lifetime.Scoped).As<IGridItemFactory>();
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