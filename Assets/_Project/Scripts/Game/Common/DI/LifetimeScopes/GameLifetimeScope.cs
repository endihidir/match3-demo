using Core.Bootstrapper;
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
            
            builder.Register<GridItemFactory>(Lifetime.Scoped).As<IGridItemFactory>();
            builder.Register<ItemAnimationFactory>(Lifetime.Scoped).As<IItemAnimationFactory>();
            
            builder.Register<LevelGoalModel>(Lifetime.Scoped).As<ILevelGoalModel>();
            builder.Register<GridModel>(Lifetime.Scoped).As<IGridModel>();
            
            builder.Register<GridView>(Lifetime.Scoped).As<IGridView>();
            
            builder.Register<GridPresenter>(Lifetime.Scoped).As<IGridPresenter>();
        }

        private void Start()
        {
            Build();
        }
    }
}