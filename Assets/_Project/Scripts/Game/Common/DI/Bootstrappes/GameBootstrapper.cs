using Core.Builder;
using Core.Level;
using Core.Presenters;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameBootstrapper : IInitializable
    {
        private readonly ICurrentLevelProvider _levelProvider;
        private readonly IGameViewContext _gameViewContext;
        private readonly IGridBuilder _gridBuilder;
        private readonly IGridPresenter _gridPresenter;
        
        public GameBootstrapper(ICurrentLevelProvider levelProvider, IGridBuilder gridBuilder, IGameViewContext gameViewContext, IGridPresenter gridPresenter)
        {
            _levelProvider = levelProvider;
            _gameViewContext = gameViewContext;
            _gridBuilder = gridBuilder;
            _gridPresenter = gridPresenter;
        }
        
        public void Initialize()
        {
            var gridBuildResult = _gridBuilder.WithView(_gameViewContext.GridView)
                                              .WithGridSize(_levelProvider.GetGridSize())
                                              .WithObjectTypes(_levelProvider.GetGridObjectTypes())
                                              .Build();

            _gridPresenter.Initialize(gridBuildResult.Model, gridBuildResult.Layout);
        }
    }
}