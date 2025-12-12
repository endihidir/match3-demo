using Core.Context;
using Core.Presenters;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class LoadingBootstrapper : IInitializable
    {
        private readonly ILoadingViewContext _loadingViewContext;
        private readonly ISceneTransitionPresenter _sceneTransitionPresenter;
        
        public LoadingBootstrapper(ILoadingViewContext loadingViewContext, ISceneTransitionPresenter sceneTransitionPresenter)
        {
            _loadingViewContext = loadingViewContext;
            _sceneTransitionPresenter = sceneTransitionPresenter;
        }
        
        public void Initialize()
        {
            _sceneTransitionPresenter.Initialize(_loadingViewContext.SceneTransitionView);
        }
    }
}