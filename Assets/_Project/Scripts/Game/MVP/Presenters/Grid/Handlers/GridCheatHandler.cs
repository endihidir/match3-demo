using Core.Extensions;
using Core.Item;
using Core.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using VContainer.Unity;

namespace Core.Handlers
{
    public sealed class GridCheatHandler : IGridCheatHandler, ITickable
    {
        private readonly IGridStateHandler _gridStateHandler;
        public GridCheatHandler(IGridStateHandler gridStateHandler) => _gridStateHandler = gridStateHandler;

        public void Tick()
        {
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
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (_gridStateHandler != null) 
                    EditorLogger.LogError(_gridStateHandler.StateMachine.CurrentState.StateID);
            }

#if UNITY_EDITOR
            
            if (Input.GetKeyDown(KeyCode.P)) EditorApplication.isPaused = !EditorApplication.isPaused;
#endif
        }

        public void Cleanup<T>() where T : BaseGridObject
        {
            if (!TryGetContext(out var context)) return;

            for (int x = 0; x < context.GridModel.Width; x++)
            {
                for (int y = 0; y < context.GridModel.Height; y++)
                {
                    var coord = new Vector2Int(x, y);
                    if (context.GridModel.GetGridObject(coord) is T)
                    {
                        ClearCell(context, coord);
                        ForceRefill();
                    }
                }
            }
        }
        
        public void GenerateItemAtMousePos(ItemType type)
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);

            var obstacle = context.GridItemFactory.GetRegularItem(type);
            PlaceItem(context, coord, obstacle);
        }

        public void GenerateBoosterAtMousePos(BoosterType type)
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);

            var booster = context.GridItemFactory.GetBoosterItem(type);
            PlaceItem(context, coord, booster);
        }

        public void GenerateObstacleAtMousePos(ObstacleType type)
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);

            var obstacle = context.GridItemFactory.GetObstacleItem(type);
            PlaceItem(context, coord, obstacle);
        }

        public void RemoveAtMousePos()
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);
        }

        public void ForceRefill() => _gridStateHandler.StateMachine?.ForceState<FillResolveState>();

        private bool TryGetContext(out GridStateContext context)
        {
            context = _gridStateHandler.Context;
            return context != null && _gridStateHandler.StateMachine != null;
        }

        private Vector2Int GetMouseGridCoord(GridStateContext context)
        {
            var screen = context.GridView.ScreenToGridCoordinate(Input.mousePosition);
            return context.GridView.InputToGridDirection(screen);
        }

        private void ClearCell(GridStateContext context, Vector2Int coord)
        {
            var obj = context.GridModel.GetGridObject(coord);
            if (!obj) return;

            context.ReleaseAndSetNull(obj, coord);
        }

        private void PlaceItem(GridStateContext context, Vector2Int coord, BaseGridObject item)
        {
            context.GridModel.SetGridObject(coord, item);
            item.SetPosition(context.GridView.GridToWorld(coord));
            item.SetSpriteSize(context.GridView.GetCellSize());
            item.SetParent(context.GridView.GridObjectsParent);
        }
    }
}