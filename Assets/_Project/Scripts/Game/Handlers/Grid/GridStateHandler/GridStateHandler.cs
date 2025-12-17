using System.Collections.Generic;
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

        private readonly AcceptMoveState _acceptMove;
        private readonly ExecuteMoveState _executeMove;
        private readonly ResolveState _resolve;
        private readonly ShiftRefillState _shiftRefill;

        private readonly Queue<GridMove> _moveQueue = new();

        public GridStateHandler(IGridModel gridModel, IGridView gridView)
        {
            _stateMachine = new StateMachine();

            _context = new GridContext(gridModel, gridView, _moveQueue);

            _acceptMove = new AcceptMoveState().Init(_context) as AcceptMoveState;
            _executeMove = new ExecuteMoveState().Init(_context) as ExecuteMoveState;
            _resolve = new ResolveState().Init(_context) as ResolveState;
            _shiftRefill = new ShiftRefillState().Init(_context) as ShiftRefillState;

            _stateMachine
                .Register(_acceptMove)
                .Register(_executeMove)
                .Register(_resolve)
                .Register(_shiftRefill);

            _stateMachine
                .AddTransition(_acceptMove, _executeMove, () => _moveQueue.Count > 0)
                .AddTransition(_executeMove, _resolve, () => _executeMove.IsExitReady)
                .AddTransition(_resolve, _shiftRefill, () => _resolve.IsExitReady)
                .AddTransition(_shiftRefill, _resolve, () => _shiftRefill.ResolveAgainRequested)
                .AddTransition(_shiftRefill, _acceptMove, () => _shiftRefill.IsExitReady);

            _stateMachine.SetInitialState(_acceptMove);
        }

        public bool TryEnqueueSwap(Vector2Int a, Vector2Int b)
        {
            if (a == b) return false;
            if (!_context.Model.IsInRange(a) || !_context.Model.IsInRange(b)) return false;

            _moveQueue.Enqueue(new GridMove(GridMoveType.Swap, a, b));
            return true;
        }

        public void Tick() => _stateMachine.Update(Time.deltaTime);
        public void FixedTick() => _stateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => _stateMachine.LateUpdate(Time.deltaTime);
    }
}