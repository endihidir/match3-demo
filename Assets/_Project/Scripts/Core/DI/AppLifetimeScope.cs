using Core.Bootstrapper;
using Core.Configs;
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
            AppConfigContainer?.Initialize();
            
            builder.RegisterInstance(AppConfigContainer);
            
            builder.RegisterEntryPoint<AppBootstrapper>();

            RegisterServices(builder);
            RegisterProviders(builder);
            RegisterGlobalModels(builder);
        }

        private static void RegisterServices(IContainerBuilder builder)
        {
            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService, ISceneLoadEvents, ISceneLoadData, ITickable>();
            builder.Register<ObjectPoolService>(Lifetime.Singleton).As<IObjectPoolService>();
            builder.Register<LevelDataService>(Lifetime.Singleton).As<IInitializable, ILevelDataBootState, ILevelSerializer, ILevelDataReader>();
            builder.Register<DataPersistenceService>(Lifetime.Singleton).As<IDataPersistenceService>();
        }

        private static void RegisterProviders(IContainerBuilder builder)
        {
            builder.Register<LevelDefinitionProvider>(Lifetime.Singleton).As<ILevelDefinitionProvider>();
        }

        private static void RegisterGlobalModels(IContainerBuilder builder)
        {
            builder.Register<LevelProgressModel>(Lifetime.Singleton).As<ILevelProgressReader, ILevelProgressWriter>();
        }
    }
}