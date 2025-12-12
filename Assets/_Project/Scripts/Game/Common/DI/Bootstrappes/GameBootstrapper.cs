using Core.Level;
using Core.Models;
using Core.Presenters;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameBootstrapper : IInitializable
    {
        private readonly LevelDefinition _levelDefinition;
        private readonly IGridPresenter _gridPresenter;
        private readonly IGameViewContext _gameViewContext;
        
        public GameBootstrapper(ILevelDataService levelDataService, ILevelProgressReadModel progressReadModel, IGridPresenter gridPresenter, 
            IGameViewContext gameViewContext)
        {
            _levelDefinition = levelDataService.LevelDefinitions[progressReadModel.CurrentLevelIndex];
            _gridPresenter =  gridPresenter;
            _gameViewContext = gameViewContext;
        }
        
        public void Initialize()
        {
           _gridPresenter.Initialize(_gameViewContext.GridView, _levelDefinition);
        }
    }
}