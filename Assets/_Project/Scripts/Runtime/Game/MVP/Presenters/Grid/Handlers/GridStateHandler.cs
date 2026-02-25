using System;
using Game.Grid.Contexts;
using Game.Grid.Item;
using Core.StateMachineCore;
using Game.Grid.States;
using Game.Grid.Handlers.Data;
using Game.Grid.Strategies;
using Game.Level.Handlers;
using Game.Models;
using Game.Views;
using UnityEngine;
using VContainer.Unity;

namespace Game.Grid.Handlers
{
    public sealed class GridStateHandler : IGridStateHandler, ITickable, IFixedTickable, ILateTickable
    {
        private readonly GridStateContext _context;
        
        private readonly IdleState _idleState;
        private readonly InputResolveState _inputState;
        private readonly BoosterResolveState _boosterState;
        private readonly ShuffleState _shuffleState;
        private readonly MatchResolveState _matchState;
        private readonly FillResolveState _fillState;
        
        private IStateMachine _stateMachine;
        
        public string CurrentStateID => _stateMachine.CurrentState.StateID;
        public event Action OnDestructionStateComplete
        {
            add => _context.OnDestructionStateComplete += value;
            remove => _context.OnDestructionStateComplete -= value;
        }
        
        private bool HasAnyInput => _context.Inputs.Count > 0;
        private bool HasPendingBoosterActions => _context.HasPendingBoosterActions;
        private bool MatchResolveRequested => _context.MatchResolveRequested;

        public GridStateHandler(IGridModel model, IGridView view, IBoosterFxHandler boosterFxHandler, IBoosterActionBuildHandler boosterActionBuilder,
            IGridObjectDestroyHandler destroyHandler, ILevelGoalHandler handler, IFillStrategyResolver fillStrategyResolver,
            IMatchDestructionHandler matchDestructionHandler, IMatchMergeHandler matchMergeHandler)
        {
            _context = new GridStateContext(model, view);

            _idleState = new IdleState(_context);
            _inputState = new InputResolveState(_context, boosterActionBuilder, handler);
            _boosterState = new BoosterResolveState(_context, boosterFxHandler, destroyHandler, handler);
            _matchState = new MatchResolveState(_context, matchDestructionHandler, matchMergeHandler);
            _shuffleState = new ShuffleState(_context);
            _fillState = new FillResolveState(_context, fillStrategyResolver);
        }

        public void Initialize()
        {
            var states = new StateBase<GridStateContext>[] { _idleState, _inputState, _boosterState, _matchState, _fillState, _shuffleState };

            _stateMachine = new StateMachine().Register(states)
                                              .SetInitialState(_idleState);

            _stateMachine.AddTransition(_idleState, _inputState, () => HasAnyInput)
                .AddTransition(_idleState, _matchState, () => MatchResolveRequested)
                .AddTransition(_idleState, _shuffleState, () => !MatchResolveRequested && !_fillState.IsInProgress && !_shuffleState.HasAnyMove())
                .AddTransition(_shuffleState, _idleState, () => _shuffleState.IsExitReady)
                         
                .AddTransition(_inputState, _boosterState, () => _inputState.IsExitReady && HasPendingBoosterActions)
                .AddTransition(_inputState, _matchState, () => _inputState.IsExitReady && MatchResolveRequested && !HasPendingBoosterActions)
                .AddTransition(_inputState, _idleState, () => _inputState.IsExitReady && !MatchResolveRequested && !HasPendingBoosterActions)
                        
                .AddTransition(_boosterState, _matchState, () => _boosterState.IsExitReady && MatchResolveRequested)
                .AddTransition(_boosterState, _fillState, () => _boosterState.IsExitReady && !MatchResolveRequested)
                .AddTransition(_matchState, _fillState, () => _matchState.IsExitReady)
                .AddTransition(_fillState, _idleState, () => _fillState.IsExitReady);
        }

        public bool TryEnqueueInput(Vector2Int coord, Vector2Int direction)
        {
            if (_stateMachine.TryGet<ShuffleState>(out var shuffleState) && shuffleState.IsInProgress) return false;
            
            if (direction == Vector2Int.zero)
                return TryEnqueueTap(coord);

            var targetCoord = coord + direction;
            return TryEnqueueSwap(coord, targetCoord);
        }

        private bool TryEnqueueTap(Vector2Int sourceCoord)
        {
            if (!_context.GridModel.IsInRange(sourceCoord)) return false;

            var objA = _context.GridModel.GetGridObject(sourceCoord);
            if (!objA) return false;

            if (!IsInteractable(objA))
            {
                objA.Animation.Shake();
                return false;
            }

            var inputSource = new GridInputSource(GridInputType.Tap, sourceCoord);
            _context.Inputs.Enqueue(inputSource);
            return true;
        }

        private bool TryEnqueueSwap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (sourceCoord == targetCoord) return false;
            
            if (!_context.GridModel.IsInRange(sourceCoord)) return false;

            var sourceObj = _context.GridModel.GetGridObject(sourceCoord);
            var targetObj = _context.GridModel.GetGridObject(targetCoord);

            if (!sourceObj) return false;

            if (!targetObj || !_context.GridModel.IsInRange(targetCoord))
            {
                sourceObj.Animation.Shake();
                return false;
            }

            if (!IsInteractable(sourceObj) || !IsInteractable(targetObj) || 
                !IsSwapCandidate(sourceObj) || !IsSwapCandidate(targetObj))
            {
                sourceObj.Animation.Shake();
                return false;
            }

            var inputSource = new GridInputSource(GridInputType.Swap, sourceCoord, targetCoord);
            _context.Inputs.Enqueue(inputSource);
            return true;
        }
        
        private static bool IsInteractable(BaseGridObject obj) => !obj.IsFallInProgress;
        private static bool IsSwapCandidate(BaseGridObject obj) => !obj.IsStationary;
        public void ForceState<T>() where T : class, IState => _stateMachine.ForceState<T>();
        public void Tick() => _stateMachine?.Update(Time.deltaTime);
        public void FixedTick() => _stateMachine?.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => _stateMachine?.LateUpdate(Time.deltaTime);
    }
}