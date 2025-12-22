using System.Collections.Generic;
using Core.Item.Factories;
using Core.Models;
using Core.StateMachineCore;
using Core.Views;
using UnityEngine;
using VContainer.Unity;

namespace Core.Handlers
{
    public interface IGridStateHandler
    {
        bool TryEnqueueSwap(Vector2Int a, Vector2Int b);
    }

    public sealed class GridStateHandler : IGridStateHandler, ITickable, IFixedTickable, ILateTickable
    {
        private readonly IStateMachine _stateMachine;
        private readonly GridContext _context;

        private readonly AcceptMoveState _accept;
        private readonly ExecuteMoveState _execute;
        private readonly ResolveState _resolve;
        private readonly FillState _fill;
        
        private readonly Queue<GridMove> _moveQueue = new();

        public GridStateHandler(IGridModel model, IGridView view, IGridItemFactory factory)
        {
            _stateMachine = new StateMachine();
            _context = new GridContext(model, view, factory, _moveQueue);
            
            _accept = new AcceptMoveState().Init(_context) as AcceptMoveState;
            _execute = new ExecuteMoveState().Init(_context) as ExecuteMoveState;
            _resolve = new ResolveState().Init(_context) as ResolveState;
            _fill = new FillState().Init(_context) as FillState;

            var states = new StateBase<GridContext>[] { _accept, _execute, _resolve, _fill };
            _stateMachine.Register(states);

            _stateMachine.AddTransition(_accept, _execute, () => _moveQueue.Count > 0 && !_context.CascadeInProgress)
                         .AddTransition(_execute, _resolve, () => _execute.IsExitReady)
                         .AddTransition(_execute, _accept, () => _execute.IsExitReady)
                         .AddTransition(_accept, _resolve, () => _context.CascadeResolveRequested && !_context.CascadeInProgress)
                         .AddTransition(_resolve, _fill, () => _resolve.IsExitReady && _context.ResolvedAnyMatch)
                         .AddTransition(_resolve, _accept, () => _resolve.IsExitReady && !_context.ResolvedAnyMatch)
                         .AddTransition(_fill, _accept, () => _fill.IsExitReady);

            _stateMachine.SetInitialState(_accept);
        }

        public bool TryEnqueueSwap(Vector2Int a, Vector2Int b)
        {
            if (a == b) return false;
            if (!_context.Model.IsInRange(a) || !_context.Model.IsInRange(b)) return false;

            var objA = _context.Model.GetGridObject(a);
            var objB = _context.Model.GetGridObject(b);

            if (!objA || !objB) return false;

            if (objA.IsShiftInProgress || objB.IsShiftInProgress)
                return false;

            //if (_context.CascadeInProgress) return false;

            _moveQueue.Enqueue(new GridMove(GridMoveType.Swap, a, b));
            return true;
        }

        public void Tick() => _stateMachine.Update(Time.deltaTime);
        public void FixedTick() => _stateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => _stateMachine.LateUpdate(Time.deltaTime);
    }
}
