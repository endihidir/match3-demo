using System;
using Game.Configs;
using Game.Grid.Contexts;
using Game.Grid.Item;
using Core.StateMachineCore;
using Game.Grid.States;
using Game.Grid.Handlers.Data;
using Game.Grid.Strategies;
using Game.Models;
using Game.Views;
using UnityEngine;
using VContainer.Unity;

namespace Game.Grid.Handlers
{
    public sealed class GridStateHandler : IGridStateHandler, ITickable, IFixedTickable, ILateTickable
    {
        private readonly IStateMachine _stateMachine;
        private readonly GridStateContext _context;
        
        public string CurrentStateID => _stateMachine.CurrentState.StateID;
        public event Action OnDestructionStateComplete
        {
            add => _context.OnDestructionStateComplete += value;
            remove => _context.OnDestructionStateComplete -= value;
        }
        
        private bool HasAnyInput => _context.Inputs.Count > 0;
        private bool HasPendingBoosterActions => _context.HasPendingBoosterActions;
        private bool MatchResolveRequested => _context.MatchResolveRequested;

        public GridStateHandler(IGridModel model, IGridView view, GridConfigContainerSO gridConfigContainer, IFillStrategyResolver fillStrategyResolver, 
            IBoosterFxHandler boosterFxHandler, IGridObjectCreateHandler objectCreateHandler, IGridObjectDestroyHandler destroyHandler, 
            ILevelGoalProgressHandler progressHandler)
        {
            var boosterComboData = gridConfigContainer.GetConfig<BoosterConfigContainerSO>().BoosterComboData;
            
            _context = new GridStateContext(model, view);

            var idleState = new IdleState(_context);
            var inputState = new InputResolveState(_context, boosterComboData, destroyHandler, progressHandler);
            var boosterState = new BoosterResolveState(_context, boosterFxHandler, destroyHandler, progressHandler);
            var matchState = new MatchResolveState(_context, objectCreateHandler, destroyHandler, progressHandler);
            var shuffleState = new ShuffleState(_context);
            var fillState = new FillResolveState(_context, fillStrategyResolver);

            var states = new StateBase<GridStateContext>[] { idleState, inputState, boosterState, matchState, fillState, shuffleState };

            _stateMachine = new StateMachine().Register(states);

            _stateMachine.AddTransition(idleState, inputState, () => HasAnyInput)
                        .AddTransition(idleState, matchState, () => MatchResolveRequested)
                        .AddTransition(idleState, shuffleState, () => !MatchResolveRequested && !fillState.IsInProgress && !shuffleState.HasAnyMove())
                        .AddTransition(shuffleState, idleState, () => shuffleState.IsExitReady)
                         
                        .AddTransition(inputState, boosterState, () => inputState.IsExitReady && HasPendingBoosterActions)
                        .AddTransition(inputState, matchState, () => inputState.IsExitReady && MatchResolveRequested && !HasPendingBoosterActions)
                        .AddTransition(inputState, idleState, () => inputState.IsExitReady && !MatchResolveRequested && !HasPendingBoosterActions)
                        
                        .AddTransition(boosterState, matchState, () => boosterState.IsExitReady && MatchResolveRequested)
                        .AddTransition(boosterState, fillState, () => boosterState.IsExitReady && !MatchResolveRequested)
                        .AddTransition(matchState, fillState, () => matchState.IsExitReady)
                        .AddTransition(fillState, idleState, () => fillState.IsExitReady);

            _stateMachine.SetInitialState(idleState);
        }

        public bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction)
        {
            if (_stateMachine.TryGet<ShuffleState>(out var shuffleState) && shuffleState.IsInProgress) return false;
            
            if (direction == Vector2Int.zero)
                return TryEnqueueTap(sourceCoord);

            var targetCoord = sourceCoord + direction;
            return TryEnqueueSwap(sourceCoord, targetCoord);
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
        
        private static bool IsInteractable(BaseGridObject obj)
        {
            if (obj.IsNone) return false;
            return !obj.IsFallInProgress;
        }
        
        private static bool IsSwapCandidate(BaseGridObject obj) => !obj.IsStationary;
        public void ForceState<T>() where T : class, IState => _stateMachine.ForceState<T>();
        public void Tick() => _stateMachine.Update(Time.deltaTime);
        public void FixedTick() => _stateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => _stateMachine.LateUpdate(Time.deltaTime);
    }
}