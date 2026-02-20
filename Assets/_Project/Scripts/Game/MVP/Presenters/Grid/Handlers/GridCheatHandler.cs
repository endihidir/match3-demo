using Core.Generated;
using Core.Scene.Services;
using Game.Grid.Item;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.States;
using Game.Models;
using Game.Views;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
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
        
        public GridCheatHandler(IGridModel gridModel, IGridView gridView, IGridStateHandler gridStateHandler, ISceneLoadService sceneLoadService, 
            IGridObjectCreateHandler objectCreateHandler, IGridObjectDestroyHandler gridObjectDestroyHandler)
        {
            _gridModel = gridModel;
            _gridView = gridView;
            _objectCreateHandler = objectCreateHandler;
            _gridStateHandler = gridStateHandler;
            _sceneLoadService = sceneLoadService;
            _gridObjectDestroyHandler = gridObjectDestroyHandler;
        }

        public void Tick()
        {
            if(!_gridView.IsInitialized) return;
            
            if (Input.GetKeyDown(KeyCode.S)) GenerateItemAtMousePos(ItemType.Blue);
            if (Input.GetKeyDown(KeyCode.L)) GenerateItemAtMousePos(ItemType.Green);
            if (Input.GetKeyDown(KeyCode.A)) GenerateItemAtMousePos(ItemType.Red);
            if (Input.GetKeyDown(KeyCode.X)) GenerateItemAtMousePos(ItemType.Yellow);
            
            if (Input.GetKeyDown(KeyCode.B)) GenerateObstacleAtMousePos(ObstacleType.Box);
            if (Input.GetKeyDown(KeyCode.N)) GenerateObstacleAtMousePos(ObstacleType.Vase);
            
            if (Input.GetKeyDown(KeyCode.T)) GenerateBoosterAtMousePos(BoosterType.Bomb);
            if (Input.GetKeyDown(KeyCode.H)) GenerateBoosterAtMousePos(BoosterType.RocketHorizontal);
            if (Input.GetKeyDown(KeyCode.V)) GenerateBoosterAtMousePos(BoosterType.RocketVertical);
            
            if (Input.GetKeyDown(KeyCode.C)) Cleanup<BoosterObject>();
            if (Input.GetKeyDown(KeyCode.O)) Cleanup<ObstacleObject>();
            if (Input.GetKeyDown(KeyCode.R)) RemoveAtMousePos();
            if (Input.GetKeyDown(KeyCode.F)) ForceRefill();
            if (Input.GetKeyDown(KeyCode.Space)) LoadMainMenu();
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                EditorLogger.LogError(_gridStateHandler.CurrentStateID);
            }

#if UNITY_EDITOR
            
            if (Input.GetKeyDown(KeyCode.P)) EditorApplication.isPaused = !EditorApplication.isPaused;
#endif
        }

        public void Cleanup<T>() where T : BaseGridObject
        {
            for (int x = 0; x < _gridModel.Width; x++)
            {
                for (int y = 0; y < _gridModel.Height; y++)
                {
                    var coord = new Vector2Int(x, y);
                    
                    if (_gridModel.GetGridObject(coord) is T)
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
            if(!IsCellActive(coord)) return;
            ClearCell(coord);

            var obstacle = _objectCreateHandler.CreateItem(type);
            PlaceItem(coord, obstacle);
        }

        public void GenerateBoosterAtMousePos(BoosterType type)
        {
            var coord = GetMouseGridCoord();
            if(!IsCellActive(coord)) return;
            ClearCell(coord);

            var booster = _objectCreateHandler.CreateBooster(type);
            PlaceItem(coord, booster);
        }

        public void GenerateObstacleAtMousePos(ObstacleType type)
        {
            var coord = GetMouseGridCoord();
            if(!IsCellActive(coord)) return;
            ClearCell(coord);

            var obstacle = _objectCreateHandler.CreateObstacle(type);
            PlaceItem(coord, obstacle);
        }

        public void RemoveAtMousePos()
        {
            var coord = GetMouseGridCoord();
            if(!IsCellActive(coord)) return;
            ClearCell(coord);
        }

        public void ForceRefill() => _gridStateHandler.ForceState<FillResolveState>();

        private Vector2Int GetMouseGridCoord() => _gridView.ScreenToGridCoordinate(Input.mousePosition);

        private void ClearCell(Vector2Int coord)
        {
            var obj = _gridModel.GetGridObject(coord);
            if (!obj) return;
            _gridObjectDestroyHandler.DestroyGridObject(obj, coord);
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