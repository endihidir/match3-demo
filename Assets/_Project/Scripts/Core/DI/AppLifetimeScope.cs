using Core.Bootstrapper;
using Core.Configs;
using Core.Level;
using Core.MVPContext;
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

            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService, ISceneLoadEvents, ISceneLoadInfo, ITickable>();
            
            builder.Register<ObjectPoolService>(Lifetime.Singleton).As<IObjectPoolService>();
            
            builder.Register<LevelDataService>(Lifetime.Singleton).As<ILevelDataService, IInitializable>();
            
            builder.Register<MVPContextService>(Lifetime.Singleton).As<IMVPContextService, ITickable>();
            
            builder.Register<DataPersistenceService>(Lifetime.Singleton).As<IDataPersistenceService>();
            
            builder.Register<GlobalModelService>(Lifetime.Singleton).As<IGlobalModelService>();
            
        }
    }
}