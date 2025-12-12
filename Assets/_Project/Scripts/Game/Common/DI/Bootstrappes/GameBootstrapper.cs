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
        
        public GameBootstrapper(ILevelProgressReadModel progressReadModel, IGridPresenter gridPresenter, IGameViewContext gameViewContext)
        {
            _levelDefinition = progressReadModel.GetLevelDefinition();
            _gridPresenter =  gridPresenter;
            _gameViewContext = gameViewContext;
        }
        
        public void Initialize()
        {
           _gridPresenter.Initialize(_gameViewContext.GridView, _levelDefinition);
        }
    }
}