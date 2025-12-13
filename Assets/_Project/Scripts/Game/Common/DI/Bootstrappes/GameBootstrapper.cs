using Core.Builder;
using Core.Level;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameBootstrapper : IInitializable
    {
        private readonly ICurrentLevelProvider _levelProvider;
        private readonly IGameViewContext _gameViewContext;
        private readonly IGridBuilder _gridBuilder;
        
        public GameBootstrapper(ICurrentLevelProvider levelProvider, IGridBuilder gridBuilder, IGameViewContext gameViewContext)
        {
            _levelProvider = levelProvider;
            _gameViewContext = gameViewContext;
            _gridBuilder = gridBuilder;
        }
        
        public void Initialize()
        {
            var gridBuildResult = _gridBuilder.WithView(_gameViewContext.GridView)
                                              .WithGridSize(_levelProvider.GetGridSize())
                                              .WithObjectTypes(_levelProvider.GetGridObjectTypes())
                                              .Build();

            //TODO: Send result to handlers
        }
    }
}