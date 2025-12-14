using Core.Handlers;
using Core.Level;
using Core.Models;
using Core.Views;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameBootstrapper : IInitializable
    {
        private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        private readonly IGridItemFactoryHandler _gridItemFactoryHandler;
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        
        public GameBootstrapper(ILevelDefinitionProvider levelDefinitionProvider, IGridItemFactoryHandler gridItemFactoryHandler, IGridModel gridModel, IGridView gridView)
        {
            _levelDefinitionProvider = levelDefinitionProvider;
            _gridItemFactoryHandler = gridItemFactoryHandler;
            _gridModel = gridModel;
            _gridView = gridView;
        }
        
        public void Initialize()
        {
            DisableInput();
            GridSetup();
            HudSetup();
            EnableInput();
        }

        private void DisableInput()
        {
            
        }
        
        private void GridSetup()
        {
            _gridItemFactoryHandler.PopulateGridWith(_levelDefinitionProvider.GetGridObjectTypes(), out var gridItemObjects);
            var width = gridItemObjects.GetLength(0);
            var height = gridItemObjects.GetLength(1);
            _gridModel.Initialize(gridItemObjects, width, height, out var activeCells);
            _gridView.Initialize(width, height, activeCells);
        }

        private void HudSetup()
        {
            
        }

        private void EnableInput()
        {
            
        }
    }
}