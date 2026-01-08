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
            if (Input.GetKeyDown(KeyCode.B)) GenerateObstacleAtMousePos(ObstacleType.Box);
            if (Input.GetKeyDown(KeyCode.N)) GenerateObstacleAtMousePos(ObstacleType.Vase);
            if (Input.GetKeyDown(KeyCode.T)) GenerateBoosterAtMousePos(BoosterType.Bomb);
            if (Input.GetKeyDown(KeyCode.H)) GenerateBoosterAtMousePos(BoosterType.RocketHorizontal);
            if (Input.GetKeyDown(KeyCode.V)) GenerateBoosterAtMousePos(BoosterType.RocketVertical);
            if (Input.GetKeyDown(KeyCode.C)) Cleanup<BoosterObject>();
            if (Input.GetKeyDown(KeyCode.O)) Cleanup<ObstacleObject>();
            if (Input.GetKeyDown(KeyCode.R)) RemoveAtMousePos();
            if (Input.GetKeyDown(KeyCode.F)) ForceRefill();
            
            if (Input.GetKeyDown(KeyCode.L))
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

            for (int x = 0; x < context.Model.Width; x++)
            {
                for (int y = 0; y < context.Model.Height; y++)
                {
                    var coord = new Vector2Int(x, y);
                    if (context.Model.GetGridObject(coord) is T itemObject)
                    {
                        ClearCell(context, coord);
                        _gridStateHandler.StateMachine.ForceState<RefillResolveState>();
                    }
                }
            }
        }

        public void GenerateBoosterAtMousePos(BoosterType type)
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);

            var booster = context.Factory.GetBoosterItem(type);
            PlaceItem(context, coord, booster);
        }

        public void GenerateObstacleAtMousePos(ObstacleType type)
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);

            var obstacle = context.Factory.GetObstacleItem(type);
            PlaceItem(context, coord, obstacle);
        }

        public void RemoveAtMousePos()
        {
            if (!TryGetContext(out var context)) return;

            var coord = GetMouseGridCoord(context);
            ClearCell(context, coord);
        }

        public void ForceRefill() => _gridStateHandler.StateMachine?.ForceState<RefillResolveState>();

        private bool TryGetContext(out GridStateContext context)
        {
            context = _gridStateHandler.Context;
            return context != null && _gridStateHandler.StateMachine != null;
        }

        private Vector2Int GetMouseGridCoord(GridStateContext context)
        {
            var screen = context.View.ScreenToGridCoordinate(Input.mousePosition);
            return context.View.InputDirectionToGridDirection(screen);
        }

        private void ClearCell(GridStateContext context, Vector2Int coord)
        {
            var obj = context.Model.GetGridObject(coord);
            if (!obj) return;

            context.Factory.ReleaseItem(obj);
            context.Model.SetGridObject(coord, null);
        }

        private void PlaceItem(GridStateContext context, Vector2Int coord, BaseGridObject item)
        {
            context.Model.SetGridObject(coord, item);
            item.SetPosition(context.View.GridToWorld(coord));
            item.SetSpriteSize(context.View.GetCellSize());
            item.SetParent(context.View.GridObjectsParent);
        }
    }
}