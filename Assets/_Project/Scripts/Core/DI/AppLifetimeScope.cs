using Core.Bootstrapper;
using Core.Configs;
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
        [SerializeField] private AppConfigContainer _appConfigContainer;
        
        protected override void Configure(IContainerBuilder builder)
        {
            _appConfigContainer?.Initialize();
            
            builder.RegisterInstance(_appConfigContainer);
            
            builder.RegisterEntryPoint<AppBootstrapper>();

            builder.Register<SceneLoadService>(Lifetime.Singleton).As<ISceneLoadService>();
            
            builder.Register<ObjectPoolService>(Lifetime.Singleton).As<IObjectPoolService>();
            
            builder.Register<SaveService>(Lifetime.Singleton).As<ISaveService>();
            
            builder.Register<MVPContextContainer>(Lifetime.Singleton).As<IMVPContextContainer, ITickable>();
            
            builder.Register<GlobalModelContainer>(Lifetime.Singleton).As<IGlobalModelContainer>();
        }
    }
}
