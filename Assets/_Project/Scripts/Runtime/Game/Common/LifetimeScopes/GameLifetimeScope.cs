using Game.Bootstrappers;
using Game.Configs;
using Game.Grid.Item.Factories;
using Game.Grid.Strategies;
using Game.Grid.Strategies.Schedulers;
using Game.Grid.Handlers;
using Game.Grid.Services;
using Game.HUD.Handlers;
using Game.Level.Handlers;
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
    public class GameplayLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private GameplayConfigContainerSO GameplayConfigContainerSo { get; set; }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(GameplayConfigContainerSo.GridConfigContainer);
            builder.RegisterEntryPoint<GameplayBootstrapper>();
    
            // SERVICE
            builder.Register<GameplaySetupService>(Lifetime.Scoped).As<IGameplaySetupService>();
            builder.Register<GridInputService>(Lifetime.Scoped).As<IGridInputService, ITickable>();
    
            // LEVEL
            builder.Register<LevelGoalModel>(Lifetime.Scoped).As<ILevelGoalModel>();
            builder.RegisterComponentInHierarchy<LevelEndView>().As<ILevelEndView>();
            builder.RegisterEntryPoint<LevelEndPresenter>();
    
            // HUD
            builder.Register<GoalSlotHandler>(Lifetime.Scoped).As<IGoalSlotHandler>();
            builder.Register<GoalFxHandler>(Lifetime.Scoped).As<IGoalFxHandler>();
            
            builder.RegisterComponentInHierarchy<HudView>().As<IHudView>();
            builder.RegisterEntryPoint<HudPresenter>();
    
            // GRID
            builder.Register<GridObjectFactory>(Lifetime.Scoped).As<IGridObjectFactory>();
            builder.Register<GridObjectCreateHandler>(Lifetime.Scoped).As<IGridObjectCreateHandler>();
            builder.Register<GridObjectDestroyHandler>(Lifetime.Scoped).As<IGridObjectDestroyHandler>();
            
            builder.Register<LevelGoalHandler>(Lifetime.Scoped).As<ILevelGoalHandler>();
            builder.Register<LevelResultHandler>(Lifetime.Scoped).As<ILevelResultHandler>();
            
            builder.Register<BlastFxHandler>(Lifetime.Scoped).As<IBlastFxHandler>();
            builder.Register<MatchDestructionHandler>(Lifetime.Scoped).As<IMatchDestructionHandler>();
            builder.Register<MatchMergeHandler>(Lifetime.Scoped).As<IMatchMergeHandler>();
            
            builder.Register<BoosterFxHandler>(Lifetime.Scoped).As<IBoosterFxHandler>();
            builder.Register<BoosterActionBuildHandler>(Lifetime.Scoped).As<IBoosterActionBuildHandler>();
            
            builder.Register<GridCheatHandler>(Lifetime.Scoped).As<IGridCheatHandler, ITickable>();
            builder.Register<GridStateHandler>(Lifetime.Scoped).As<IGridStateHandler, ITickable, IFixedTickable, ILateTickable>();
            
            builder.Register<GridModel>(Lifetime.Scoped).As<IGridModel>();
            builder.RegisterComponentInHierarchy<GridView>().As<IGridView>();
            builder.RegisterEntryPoint<GridPresenter>();
    
            // GRID FILL
            builder.Register<SlideAnimationScheduler>(Lifetime.Scoped).As<ISlideAnimationScheduler>();
            builder.Register<FallAnimationScheduler>(Lifetime.Scoped).As<IFallAnimationScheduler>();
            
            builder.Register<FillItemDecider>(Lifetime.Scoped).As<IFillItemDecider>();
            builder.Register<SlideDownFillStrategy>(Lifetime.Scoped).As<IFillStrategy>();
            builder.Register<FallDownFillStrategy>(Lifetime.Scoped).As<IFillStrategy>();
            builder.Register<FillStrategyResolver>(Lifetime.Scoped).As<IFillStrategyResolver>();
        }
    }
}