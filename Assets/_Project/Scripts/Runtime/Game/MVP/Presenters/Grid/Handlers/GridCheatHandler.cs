using Core.Generated;
using Core.Scene.Services;
using Game.Grid.Item;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.States;
using Game.Level.Handlers;
using Game.Models;
using Game.Services;
using Game.Views;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace Game.Grid.Handlers
{
    public sealed class GridCheatHandler : IGridCheatHandler, ITickable
    {
        private readonly IGridModel _gridModel;
        private readonly IGridView _gridView;
        private readonly IGridStateHandler _gridStateHandler;
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IGridObjectCreateHandler _objectCreateHandler;
        private readonly IGridObjectDestroyHandler _gridObjectDestroyHandler;
        private readonly ILevelGoalHandler _levelGoalHandler;
        private readonly IGameplaySetupService _gameplaySetupService;
        
        public GridCheatHandler(IGridModel gridModel, IGridView gridView, IGridStateHandler gridStateHandler, ISceneLoadService sceneLoadService, 
            IGridObjectCreateHandler objectCreateHandler, IGridObjectDestroyHandler gridObjectDestroyHandler, ILevelGoalHandler levelGoalHandler,
            IGameplaySetupService gameplaySetupService)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _gridStateHandler = gridStateHandler;
            _sceneLoadService = sceneLoadService;
            _gridObjectDestroyHandler = gridObjectDestroyHandler;
            _levelGoalHandler = levelGoalHandler;
            _gameplaySetupService = gameplaySetupService;
        }

        public void Tick()
        {
            if (!_gridView.IsInitialized) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.sKey.wasPressedThisFrame) GenerateItemAtMousePos(ItemType.Blue);
            if (kb.lKey.wasPressedThisFrame) GenerateItemAtMousePos(ItemType.Green);
            if (kb.aKey.wasPressedThisFrame) GenerateItemAtMousePos(ItemType.Red);
            if (kb.xKey.wasPressedThisFrame) GenerateItemAtMousePos(ItemType.Yellow);

            if (kb.bKey.wasPressedThisFrame) GenerateObstacleAtMousePos(ObstacleType.Box);
            if (kb.nKey.wasPressedThisFrame) GenerateObstacleAtMousePos(ObstacleType.Vase);

            if (kb.tKey.wasPressedThisFrame) GenerateBoosterAtMousePos(BoosterType.Bomb);
            if (kb.hKey.wasPressedThisFrame) GenerateBoosterAtMousePos(BoosterType.RocketHorizontal);
            if (kb.vKey.wasPressedThisFrame) GenerateBoosterAtMousePos(BoosterType.RocketVertical);

            if (kb.cKey.wasPressedThisFrame) Cleanup<BoosterObject>();
            if (kb.oKey.wasPressedThisFrame) Cleanup<ObstacleObject>();
            if (kb.rKey.wasPressedThisFrame) RemoveAtMousePos();
            if (kb.fKey.wasPressedThisFrame) ForceRefill();
            if (kb.spaceKey.wasPressedThisFrame) LoadMainMenu();

            if (kb.dKey.wasPressedThisFrame)
            {
                EditorLogger.LogError(_gridStateHandler.CurrentStateID);
            }

#if UNITY_EDITOR
            if (kb.pKey.wasPressedThisFrame) EditorApplication.isPaused = !EditorApplication.isPaused;
#endif
        }

        public void Cleanup<T>() where T : BaseGridObject
        {
            for (int x = 0; x < _gridModel.Width; x++)
            {
                for (int y = 0; y < _gridModel.Height; y++)
                {
                    var coord = new Vector2Int(x, y);
                    
                    var obj = _gridModel.GetGridObject(coord);
                    
                    if (obj is T)
                    {
                        ClearCell(coord);
                        ForceRefill();
                    }
                }
            }
        }
        
        public void GenerateItemAtMousePos(ItemType type)
        {
            var coord = GetMouseGridCoord();
            if (!IsCellActive(coord)) return;
            ClearCell(coord);

            var obstacle = _objectCreateHandler.CreateItem(type);
            PlaceItem(coord, obstacle);
        }

        public void GenerateBoosterAtMousePos(BoosterType type)
        {
            var coord = GetMouseGridCoord();
            if (!IsCellActive(coord)) return;
            ClearCell(coord);

            var booster = _objectCreateHandler.CreateBooster(type);
            PlaceItem(coord, booster);
        }

        public void GenerateObstacleAtMousePos(ObstacleType type)
        {
            var coord = GetMouseGridCoord();
            if (!IsCellActive(coord)) return;
            ClearCell(coord);

            var obstacle = _objectCreateHandler.CreateObstacle(type);
            _gameplaySetupService.AddNewGoal(obstacle.ObjectType, 1);
            PlaceItem(coord, obstacle);
        }

        public void RemoveAtMousePos()
        {
            var coord = GetMouseGridCoord();
            if (!IsCellActive(coord)) return;
            ClearCell(coord);
        }

        public void ForceRefill() => _gridStateHandler.ForceState<FillResolveState>();

        private Vector2Int GetMouseGridCoord()
        {
            var mousePos = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            return _gridView.ScreenToGrid(mousePos);
        }

        private void ClearCell(Vector2Int coord)
        {
            var obj = _gridModel.GetGridObject(coord);
            
            if (!obj) return;
            
            if(obj is IDamageableGridObject)
                _levelGoalHandler.ProgressGoal(obj);
            
            _gridObjectDestroyHandler.DestroyGridObject(obj);
        }

        private bool IsCellActive(Vector2Int coord) => _gridModel.IsCellActive(coord);

        private void PlaceItem(Vector2Int coord, BaseGridObject item)
        {
            _gridModel.SetGridObject(coord, item);
            item.SetPosition(_gridView.GridToWorld(coord));
            item.SetSpriteSize(_gridView.GetCellSize());
            item.SetParent(_gridView.GridObjectsParent);
        }

        private void LoadMainMenu() => _sceneLoadService.LoadSceneGroupAsync(SceneGroupType.MenuScene, true).Forget();
    }
}