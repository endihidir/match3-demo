using System;
using Core.Bootstrapper;
using Core.Configs;
using Core.Item.Factories;
using Core.Models;
using Core.Presenters;
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
            
            builder.Register<Match3GridModel>(Lifetime.Scoped).As<IMatch3GridModel>();
            
            builder.Register<Match3GridPresenter>(Lifetime.Scoped).As<IMatch3GridPresenter>();
        }

        private void Start()
        {
            Build();
        }
    }
}