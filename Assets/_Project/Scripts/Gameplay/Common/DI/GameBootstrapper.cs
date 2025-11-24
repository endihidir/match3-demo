using Core.SceneService;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameBootstrapper : IInitializable
    {
        private readonly IGridPresenter _gridPresenter;

        public GameBootstrapper(IGridPresenter gridPresenter)
        {
            _gridPresenter = gridPresenter;
        }
        
        public void Initialize()
        {
           // _gridPresenter.Initialize();
        }
    }
}