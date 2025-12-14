using System;
using Core.Level;
using Core.Presenters;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class MenuBootstrapper : IInitializable, IDisposable
    {
        private readonly IMainMenuPresenter _mainMenuPresenter;
        private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        
        public MenuBootstrapper(ILevelDefinitionProvider levelDefinitionProvider, IMainMenuPresenter mainMenuPresenter)
        {
            _levelDefinitionProvider = levelDefinitionProvider;
            _mainMenuPresenter = mainMenuPresenter;
        }
        
        public void Initialize()
        {
            _mainMenuPresenter.InitPlayButton(_levelDefinitionProvider.GetLevelNumber());
        }

        public void Dispose()
        {
            
        }
    }
}