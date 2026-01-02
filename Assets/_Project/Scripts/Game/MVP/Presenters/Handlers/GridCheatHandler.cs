using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using UnityEngine;
using VContainer.Unity;

namespace Core.Handlers
{
    public sealed class GridCheatHandler : IGridCheatHandler, ITickable
    {
        private readonly IGridStateHandler _gridStateHandler;
        
        public GridCheatHandler(IGridStateHandler gridStateHandler)
        {
            _gridStateHandler = gridStateHandler;
        }

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                GenerateBoosterAtMousePos(BoosterType.Bomb);
            }
            
            if (Input.GetKeyDown(KeyCode.H))
            {
                GenerateBoosterAtMousePos(BoosterType.RocketHorizontal);
            }
            
            if (Input.GetKeyDown(KeyCode.V))
            {
                GenerateBoosterAtMousePos(BoosterType.RocketVertical);
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                CleanupBoosters();
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                if (_gridStateHandler != null)
                {
                    EditorLogger.LogError(_gridStateHandler.StateMachine.CurrentState.StateID);
                }
            }
        }
        
        public void CleanupBoosters()
        {
            var context = _gridStateHandler.Context;
            var stateMachine = _gridStateHandler.StateMachine;
            
            if(context == null || stateMachine == null)  return;
            
            for (int i = 0; i < context.Model.Width; i++)
            {
                for (int j = 0; j < context.Model.Height; j++)
                {
                    var coord = new Vector2Int(i, j);
                    var obj = context.Model.GetGridObject(coord);
                    if (obj is BoosterObject boosterObject)
                    {
                        context.Factory.ReleaseItem(boosterObject);
                        context.Model.SetGridObject(coord, null);
                        stateMachine.ForceState<RefillResolveState>();
                    }
                }
            }
        }

        public void GenerateBoosterAtMousePos(BoosterType boosterType)
        {
            var context = _gridStateHandler.Context;
            var stateMachine = _gridStateHandler.StateMachine;
            
            if(context == null || stateMachine == null)  return;
            
            var pos = context.View.ScreenToGridCoordinate(Input.mousePosition);
            var actPos = context.View.InputDirectionToGridDirection(pos);
            var obj = context.Model.GetGridObject(actPos);
            if (obj)
            {
                context.Factory.ReleaseItem(obj);
                context.Model.SetGridObject(actPos, null);
                var newObj = context.Factory.GetBoosterItem(boosterType);
                context.Model.SetGridObject(actPos, newObj);
                newObj.SetPosition(context.View.GridToWorld(actPos));
                newObj.SetSpriteSize(context.View.GetCellSize());
                newObj.SetParent(context.View.GridObjectsParent);
            }
        }
    }
}