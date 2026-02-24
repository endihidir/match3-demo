using Game.Bootstrappers;
using Game.Configs;
using Core.Scene.Services;
using Core.Pool.Services;
using Core.SaveSystem;
using Game.View.Factories;
using Game.Level.Models;
using Game.Level.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Game.DI
{
    public sealed class AppLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private AppConfigContainerSO AppConfigContainerSo {get; set;}
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(AppConfigContainerSo.LevelDataServiceConfig);
            builder.RegisterInstance(AppConfigContainerSo.PoolServiceConfig);
            builder.RegisterInstance(AppConfigContainerSo.SceneLoadServiceConfig);
            
            builder.RegisterEntryPoint<AppBootstrapper>();

            RegisterServices(builder);
            RegisterModels(builder);
            RegisterFactories(builder);
        }
        
        private static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService, ISceneLoadState>();
            builder.Register<ObjectPoolService>(Lifetime.Singleton).As<IObjectPoolService>();
            builder.Register<LevelDataService>(Lifetime.Singleton).As<ILevelDataService>();
            builder.Register<JsonSaveService>(Lifetime.Singleton).As<IJsonSaveService>();
            builder.Register<LevelDefinitionProvider>(Lifetime.Singleton).As<ILevelDefinitionProvider>();
        }
        
        private static void RegisterModels(IContainerBuilder builder)
        {
            builder.Register<LevelProgressionModel>(Lifetime.Singleton).As<ILevelProgressionModel>();
        }
        
        private static void RegisterFactories(IContainerBuilder builder)
        {
            builder.Register<SlotViewFactory>(Lifetime.Singleton).As<ISlotViewFactory>();
            builder.Register<FXViewFactory>(Lifetime.Singleton).As<IFXViewFactory>();
        }
    }
}