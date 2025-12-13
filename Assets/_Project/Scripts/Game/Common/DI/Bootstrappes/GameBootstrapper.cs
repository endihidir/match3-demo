using Core.Builder;
using Core.Level;
using Core.Presenters;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameBootstrapper : IInitializable
    {
        private readonly ICurrentLevelProvider _currentLevelProvider;
        private readonly IGameViewContext _gameViewContext;
        private readonly IGridBuilder _gridBuilder;
        private readonly IGridPresenter _gridPresenter;
        
        public GameBootstrapper(ICurrentLevelProvider currentLevelProvider, IGridBuilder gridBuilder, IGameViewContext gameViewContext, IGridPresenter gridPresenter)
        {
            _currentLevelProvider = currentLevelProvider;
            _gameViewContext = gameViewContext;
            _gridBuilder = gridBuilder;
            _gridPresenter = gridPresenter;
        }
        
        public void Initialize()
        {
            var result = _gridBuilder.WithView(_gameViewContext.GridView)
                                     .WithGridSize(_currentLevelProvider.GetGridSize())
                                     .WithObjectTypes(_currentLevelProvider.GetGridObjectTypes())
                                     .Build();

            _gridPresenter.Initialize(result.Model, result.Layout);
        }
    }
}