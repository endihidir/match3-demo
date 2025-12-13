using System;
using Core.Models;
using Core.Presenters;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class MenuBootstrapper : IInitializable, IDisposable
    {
        private readonly IMainMenuViewContext _mainMenuViewContext;
        private readonly IMainMenuPresenter _mainMenuPresenter;
        private readonly ILevelProgressReader _progressReader;
        
        public MenuBootstrapper(ILevelProgressReader progressReader, IMainMenuViewContext mainMenuViewContext, IMainMenuPresenter mainMenuPresenter)
        {
            _progressReader = progressReader;
            _mainMenuViewContext = mainMenuViewContext;
            _mainMenuPresenter = mainMenuPresenter;
        }
        
        public void Initialize()
        {
            _mainMenuPresenter.Initialize(_mainMenuViewContext.PlayButtonView, _progressReader.DisplayLevelNumber);
        }

        public void Dispose()
        {
            
        }
    }
}