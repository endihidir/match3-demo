using System;
using Core.Presenters;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class MenuBootstrapper : IInitializable, IDisposable
    {
        private readonly IMainMenuViewContext _mainMenuViewContext;
        private readonly IMainMenuPresenter _mainMenuPresenter;
        
        public MenuBootstrapper(IMainMenuViewContext mainMenuViewContext, IMainMenuPresenter mainMenuPresenter)
        {
            _mainMenuViewContext = mainMenuViewContext;
            _mainMenuPresenter = mainMenuPresenter;
        }
        
        public void Initialize()
        {
            _mainMenuPresenter.Initialize(_mainMenuViewContext.PlayButtonView);
        }

        public void Dispose()
        {
            
        }
    }
}