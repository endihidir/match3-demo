using Core.Bootstrapper;
using Core.Builder;
using Core.Configs;
using Core.Item.Factories;
using Core.Models;
using Core.Presenters;
using Core.Views;
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
            builder.RegisterComponentInHierarchy<GameViewContext>().As<IGameViewContext>();
            
            builder.Register<LevelGoalModel>(Lifetime.Scoped).As<ILevelGoalModel>();
            
            builder.Register<GridItemFactory>(Lifetime.Scoped).As<IGridItemFactory>();
            builder.Register<ItemAnimationFactory>(Lifetime.Scoped).As<IItemAnimationFactory>();
            
            builder.Register<GridBuilder>(Lifetime.Transient).As<IGridBuilder>();
            builder.Register<GridModel>(Lifetime.Scoped).As<IGridModel>();
            builder.Register<GridPresenter>(Lifetime.Scoped).As<IGridPresenter, IInitializable>();
        }

        private void Start()
        {
            Build();
        }
    }
}