using Core.Handlers;
using Core.Level;
using Core.Models;
using Core.Services;
using Core.Views;
using DG.Tweening;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameplayBootstrapper : IPostInitializable
    {
        [Inject] private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        [Inject] private readonly IGridItemFactoryHandler _gridItemFactoryHandler;
        [Inject] private readonly IGridModel _gridModel;
        [Inject] private readonly IGridView _gridView;
        [Inject] private readonly IInputService _inputService;
        
        public void PostInitialize()
        {
            DOTween.SetTweensCapacity(2000, 500);
            DisableInput();
            HudSetup();
            GridSetup();
            EnableInput();
        }

        private void DisableInput() => _inputService.Disable();

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

        private void EnableInput() => _inputService.Enable();
    }
}