using System.Collections.Generic;
using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
using Core.StateMachineCore;
using Core.Views;
using UnityEngine;
using VContainer.Unity;

namespace Core.Handlers
{
    public sealed class GridStateHandler : IGridStateHandler, ITickable, IFixedTickable, ILateTickable
    {
        private readonly IStateMachine _stateMachine;
        private readonly GridStateContext _context;

        private readonly IdleState _idleState;
        private readonly ApplyInputState _applyInputState;
        private readonly ResolveState _resolveState;
        private readonly RefillState _refillState;

        private readonly Queue<GridMove> _moveQueue = new();

        public GridStateHandler(IGridModel model, IGridView view, IGridItemFactory factory, GameplayConfigContainer configContainer, 
            IRefillStrategyHandler refillStrategyHandler, IMatchResolveHandler matchResolveHandler)
        {
            _stateMachine = new StateMachine();
            
            var itemConfigContainer = configContainer.ItemConfigContainer;
            _context = new GridStateContext(model, view, factory, itemConfigContainer, _moveQueue);
            matchResolveHandler.Initialize(_context);

            _idleState = new IdleState().Init(_context) as IdleState;
            _applyInputState = new ApplyInputState().Init(_context) as ApplyInputState;
            _resolveState = new ResolveState(matchResolveHandler).Init(_context) as ResolveState;
            _refillState = new RefillState(refillStrategyHandler).Init(_context) as RefillState;

            var states = new StateBase<GridStateContext>[] { _idleState, _applyInputState, _resolveState, _refillState };
            _stateMachine.Register(states);

            _stateMachine.AddTransition(_idleState, _applyInputState, () => _moveQueue.Count > 0)
                         .AddTransition(_idleState, _resolveState, () => _context.RefillResolveRequested && !_context.RefillInProgress)
                         .AddTransition(_applyInputState, _idleState, () => _applyInputState.IsExitReady)
                         .AddTransition(_applyInputState, _resolveState, () => _applyInputState.IsExitReady) // TODO: Think about it!
                         .AddTransition(_resolveState, _refillState, () => _resolveState.IsExitReady && _context.ResolvedAnyMatch)
                         .AddTransition(_resolveState, _idleState, () => _resolveState.IsExitReady && !_context.ResolvedAnyMatch)
                         .AddTransition(_refillState, _idleState, () => _refillState.IsExitReady);

            _stateMachine.SetInitialState(_idleState);
        }

        public bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction)
        {
            if (direction == Vector2Int.zero)
                return TryEnqueueBooster(sourceCoord);

            var targetCoord = sourceCoord + direction;
            return TryEnqueueSwap(sourceCoord, targetCoord);
        }

        private bool TryEnqueueBooster(Vector2Int a)
        {
            if (!_context.Model.IsInRange(a)) return false;

            var objA = _context.Model.GetGridObject(a);
            if (!objA) return false;

            if (!IsSourceInteractable(objA) || objA is not BoosterObject)
            {
                objA.ItemAnimation.Shake();
                return false;
            }

            _moveQueue.Enqueue(new GridMove(GridMoveType.Tap, a));
            return true;
        }

        private bool TryEnqueueSwap(Vector2Int a, Vector2Int b)
        {
            if (a == b) return false;
            if (!_context.Model.IsInRange(a) || !_context.Model.IsInRange(b)) return false;

            var objA = _context.Model.GetGridObject(a);
            var objB = _context.Model.GetGridObject(b);

            if (!objA || !objB) return false;

            if (!IsSourceInteractable(objA) || !IsTargetSwapCandidate(objB) || !IsSourceInteractable(objB))
            {
                objA.ItemAnimation.Shake();
                return false;
            }

            _moveQueue.Enqueue(new GridMove(GridMoveType.Swap, a, b));
            return true;
        }

        private static bool IsSourceInteractable(BaseGridObject obj)
        {
            if (!obj) return false;
            if (obj.IsEmpty) return false;
            return !obj.IsShiftInProgress;
        }

        private static bool IsTargetSwapCandidate(BaseGridObject obj)
        {
            if (!obj) return false;
            if (obj.IsStationary) return false;
            return obj is not ObstacleObject;
        }

        public void Tick() => _stateMachine.Update(Time.deltaTime);
        public void FixedTick() => _stateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => _stateMachine.LateUpdate(Time.deltaTime);
    }
}