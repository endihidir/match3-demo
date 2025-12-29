using System.Collections.Generic;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Core.Views;
using UnityEngine;
using VContainer.Unity;
using StateMachine = Core.StateMachineCore.StateMachine;

namespace Core.Handlers
{
    public sealed class GridStateHandler : IGridStateHandler, ITickable, IFixedTickable, ILateTickable
    {
        private readonly IStateMachine _stateMachine;
        private readonly GridStateContext _context;

        private readonly IdleState _idleState;
        private readonly ApplyInputState _applyInputState;
        private readonly MatchResolveState _matchResolveState;
        private readonly RefillState _refillState;
        private readonly BoosterResolveState _boosterResolveState;

        private readonly Queue<GridMove> _moveQueue = new();
        
        public GridStateHandler(IGridModel model, IGridView view, IGridItemFactory factory, GameplayConfigContainer configContainer, 
            IRefillStrategyHandler strategyHandler)
        {
            _stateMachine = new StateMachine();
            
            _context = new GridStateContext(model, view, factory, configContainer.ItemConfigContainer, _moveQueue);

            _idleState = new IdleState().Init(_context) as IdleState;
            _applyInputState = new ApplyInputState().Init(_context) as ApplyInputState;
            _boosterResolveState = new BoosterResolveState().Init(_context) as BoosterResolveState;
            _matchResolveState = new MatchResolveState().Init(_context) as MatchResolveState;
            _refillState = new RefillState(strategyHandler).Init(_context) as RefillState;
            

            var states = new StateBase<GridStateContext>[] { _idleState, _applyInputState, _boosterResolveState, _matchResolveState, _refillState };
            _stateMachine.Register(states);

            _stateMachine.AddTransition(_idleState, _applyInputState, () => _moveQueue.Count > 0)
                         .AddTransition(_idleState, _matchResolveState, () => _context.RefillResolveRequested && !_context.RefillInProgress)
                         .AddTransition(_applyInputState, _boosterResolveState, () => _applyInputState.IsExitReady)
                         .AddTransition(_applyInputState, _idleState, () => _applyInputState.IsExitReady)
                         .AddTransition(_boosterResolveState, _refillState, () => _boosterResolveState.IsExitReady && _context.ResolvedAnyBooster)
                         .AddTransition(_boosterResolveState, _matchResolveState, () => _boosterResolveState.IsExitReady && !_context.ResolvedAnyBooster)
                         .AddTransition(_matchResolveState, _refillState, () => _matchResolveState.IsExitReady && _context.ResolvedAnyMatch)
                         .AddTransition(_matchResolveState, _idleState, () => _matchResolveState.IsExitReady && !_context.ResolvedAnyMatch)
                         .AddTransition(_refillState, _idleState, () => _refillState.IsExitReady);

            _stateMachine.SetInitialState(_idleState);
        }

        public bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
                return TryEnqueueTap(sourceCoord);

            var targetCoord = sourceCoord + direction;
            return TryEnqueueSwap(sourceCoord, targetCoord);
        }

        private bool TryEnqueueTap(Vector2Int sourceCoord)
        {
            if (!_context.Model.IsInRange(sourceCoord)) return false;

            var objA = _context.Model.GetGridObject(sourceCoord);
            if (!objA) return false;

            if (!IsInteractable(objA))
            {
                objA.ItemAnimation.Shake();
                return false;
            }

            _moveQueue.Enqueue(new GridMove(GridMoveType.Tap, sourceCoord));
            return true;
        }

        private bool TryEnqueueSwap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (sourceCoord == targetCoord) return false;
            
            if (!_context.Model.IsInRange(sourceCoord) || !_context.Model.IsInRange(targetCoord)) return false;

            var sourceObj = _context.Model.GetGridObject(sourceCoord);
            var targetObj = _context.Model.GetGridObject(targetCoord);

            if (!sourceObj || !targetObj) return false;

            if (!IsInteractable(sourceObj) || !IsInteractable(targetObj) || 
                !IsSwapCandidate(sourceObj) || !IsSwapCandidate(targetObj))
            {
                sourceObj.ItemAnimation.Shake();
                return false;
            }

            _moveQueue.Enqueue(new GridMove(GridMoveType.Swap, sourceCoord, targetCoord));
            return true;
        }

        private static bool IsInteractable(BaseGridObject obj)
        {
            if (obj.IsEmpty) return false;
            return !obj.IsShiftInProgress;
        }

        private static bool IsSwapCandidate(BaseGridObject obj) => !obj.IsStationary;

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
                EditorLogger.LogError(_stateMachine.CurrentState);
            }
            
            _stateMachine.Update(Time.deltaTime);
        }

        private void CleanupBoosters()
        {
            for (int i = 0; i < _context.Model.Width; i++)
            {
                for (int j = 0; j < _context.Model.Height; j++)
                {
                    var coord = new Vector2Int(i, j);
                    var obj = _context.Model.GetGridObject(coord);
                    if (obj is BoosterObject boosterObject)
                    {
                        _context.Factory.ReleaseItem(boosterObject);
                        _context.Model.SetGridObject(coord, null);
                        _stateMachine.ForceState(_refillState);
                    }
                }
            }
        }

        private void GenerateBoosterAtMousePos(BoosterType boosterType)
        {
            var pos = _context.View.ScreenToGridCoordinate(Input.mousePosition);
            var actPos = _context.View.InputDirectionToGridDirection(pos);
            var obj = _context.Model.GetGridObject(actPos);
            if (obj)
            {
                _context.Factory.ReleaseItem(obj);
                _context.Model.SetGridObject(actPos, null);
                var newObj = _context.Factory.GetBoosterItem(boosterType);
                _context.Model.SetGridObject(actPos, newObj);
                newObj.SetPosition(_context.View.GridToWorld(actPos));
                newObj.SetSpriteSize(_context.View.GetCellSize());
                newObj.SetParent(_context.View.GridObjectsParent);
            }
        }

        public void FixedTick() => _stateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => _stateMachine.LateUpdate(Time.deltaTime);
    }
}