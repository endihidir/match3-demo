using Core.Bootstrappers;
using Core.Configs;
using Core.Item.Factories;
using Core.Level;
using Core.Models;
using Core.SceneService;
using Core.Pool;
using Core.SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.LifetimeScopes
{
    public class AppLifetimeScope : LifetimeScope
    {
        [field: SerializeField] private AppConfigContainer AppConfigContainer {get; set;}
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(AppConfigContainer.LevelDataServiceConfig);
            builder.RegisterInstance(AppConfigContainer.PoolServiceConfig);
            builder.RegisterInstance(AppConfigContainer.SceneLoadServiceConfig);
            
            builder.RegisterEntryPoint<AppBootstrapper>();

            RegisterServices(builder);
            RegisterModels(builder);
            RegisterFactories(builder);
            RegisterProviders(builder);
        }
        
        private static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService, ISceneLoadState, ITickable>();
            builder.Register<ObjectPoolService>(Lifetime.Singleton).As<IObjectPoolService>();
            builder.Register<LevelDataService>(Lifetime.Singleton).As<ILevelDataService>();
            builder.Register<JsonSaveService>(Lifetime.Singleton).As<IJsonSaveService>();
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
        
        private static void RegisterProviders(IContainerBuilder builder)
        {
            builder.Register<LevelDefinitionProvider>(Lifetime.Singleton).As<ILevelDefinitionProvider>();
        }
    }
}