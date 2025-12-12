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
        private readonly ILevelProgressReadModel _progressReadModel;
        
        public MenuBootstrapper(ILevelProgressReadModel progressReadModel, IMainMenuViewContext mainMenuViewContext, IMainMenuPresenter mainMenuPresenter)
        {
            _progressReadModel = progressReadModel;
            _mainMenuViewContext = mainMenuViewContext;
            _mainMenuPresenter = mainMenuPresenter;
        }
        
        public void Initialize()
        {
            _mainMenuPresenter.Initialize(_mainMenuViewContext.PlayButtonView, _progressReadModel.DisplayLevelNumber);
        }

        public void Dispose()
        {
            
        }
    }
}