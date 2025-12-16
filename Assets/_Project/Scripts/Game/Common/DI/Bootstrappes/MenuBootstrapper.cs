using System;
using Core.Level;
using Core.Presenters;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class MenuBootstrapper : IInitializable, IDisposable
    {
        private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        private readonly IPlayButtonView _playButtonView;
        
        public MenuBootstrapper(ILevelDefinitionProvider levelDefinitionProvider, IPlayButtonView playButtonView)
        {
            _levelDefinitionProvider = levelDefinitionProvider;
            _playButtonView = playButtonView;
        }
        
        public void Initialize()
        {
            _playButtonView.SetLevelNumber(_levelDefinitionProvider.GetLevelNumber());
        }

        public void Dispose()
        {
            
        }
    }
}