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
        [field: SerializeField] private GameConfigContainer GameConfigContainer {get; set;}

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(GameConfigContainer);
            builder.RegisterEntryPoint<GameBootstrapper>();
            
            builder.Register<InputService>(Lifetime.Scoped).As<IInputService>();
            builder.Register<LevelGoalModel>(Lifetime.Scoped).As<ILevelGoalModel>();
            
            builder.Register<GridItemFactory>(Lifetime.Scoped).As<IGridItemFactory>();
            builder.Register<GridItemFactoryHandler>(Lifetime.Scoped).As<IGridItemFactoryHandler>();
            
            builder.Register<GridModel>(Lifetime.Scoped).As<IGridModel>();
            builder.RegisterComponentInHierarchy<GridView>().As<IGridView>();
            builder.Register<GridPresenter>(Lifetime.Scoped).As<IInitializable>();
        }

        private void Start()
        {
            Build();
        }
    }
}