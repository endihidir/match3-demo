using System;
using Core.Generated;
using Core.Handlers;
using Core.Level;
using Core.Models;
using Core.SceneService;
using Core.Services;
using Core.Views;
using VContainer;
using VContainer.Unity;

namespace Core.Bootstrapper
{
    public class GameplayBootstrapper : IInitializable, IDisposable
    {
        [Inject] private readonly ISceneLoadState _sceneLoadState;
        [Inject] private readonly ILevelDefinitionProvider _levelDefinitionProvider;
        [Inject] private readonly IGridItemCreator _gridItemCreator;
        [Inject] private readonly IGridModel _gridModel;
        [Inject] private readonly IGridView _gridView;
        [Inject] private readonly IInputService _inputService;
        
        public void Initialize()
        {
            _sceneLoadState.OnLoadComplete += OnSceneLoadComplete;
        }

        private void OnSceneLoadComplete()
        {
            if(_sceneLoadState.CurrentSceneGroupType != SceneGroupType.GameScene) return;
            
            DisableInput();
            
            HudSetup();
            
            GridSetup();
            
            EnableInput();
        }

        private void DisableInput() => _inputService.Disable();
        private void HudSetup()
        {
            
        }

        private void GridSetup()
        {
            var gridObjectTypes = _levelDefinitionProvider.GetGridObjectTypes();
            var width = gridObjectTypes.GetLength(0);
            var height = gridObjectTypes.GetLength(1);
            _gridItemCreator.CreateGridItems(gridObjectTypes, width, height, out var gridItemObjects);
            _gridModel.Initialize(gridItemObjects, width, height, out var activeCells);
            _gridView.Initialize(width, height, activeCells);
        }
        
        private void EnableInput() => _inputService.Enable();
        public void Dispose()
        {
            _sceneLoadState.OnLoadComplete -= OnSceneLoadComplete;
        }
    }
}