using Core.Configs;
using Core.Item;
using Core.Item.Factories;
using Core.Models;
using Core.StateMachineCore;
using Core.Views;
using UnityEngine;
using VContainer.Unity;
using StateMachine = Core.StateMachineCore.StateMachine;

namespace Core.Handlers
{
    public sealed class GridStateHandler : IGridStateHandler, ITickable, IFixedTickable, ILateTickable
    {
        public IStateMachine StateMachine { get; } = new StateMachine();
        public GridStateContext Context { get; }
        private bool HasAnyInput => Context.Inputs.Count > 0;
        private bool HasPendingBoosterActions => Context.HasPendingBoosterActions;
        private bool MatchResolveRequested => Context.MatchResolveRequested;

        public GridStateHandler(IGridModel model, IGridView view, IGridObjectHandler objectHandler, GridConfigContainerSO gridConfigContainer, 
             ILevelObjectiveModel levelObjectiveModel, IFillStrategyResolver fillStrategyResolver, IBlastFxHandler blastFxHandler, 
             IBoosterFxHandler boosterFxHandler)
        {
            Context = new GridStateContext(model, view, objectHandler, levelObjectiveModel, gridConfigContainer, blastFxHandler);

            var idleState = new IdleState(Context);
            var inputState = new InputResolveState(Context);
            var boosterState = new BoosterResolveState(Context, boosterFxHandler);
            var matchState = new MatchResolveState(Context);
            var shuffleState = new ShuffleState(Context);
            var fillState = new FillResolveState(Context, fillStrategyResolver);

            var states = new StateBase<GridStateContext>[] { idleState, inputState, boosterState, matchState, fillState, shuffleState };
            
            StateMachine.Register(states);

            StateMachine.AddTransition(idleState, inputState, () => HasAnyInput)
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

            StateMachine.SetInitialState(idleState);
        }

        public bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction)
        {
            if (StateMachine.TryGet<ShuffleState>(out var shuffleState) && shuffleState.IsInProgress) return false;
            
            if (direction == Vector2Int.zero)
                return TryEnqueueTap(sourceCoord);

            var targetCoord = sourceCoord + direction;
            return TryEnqueueSwap(sourceCoord, targetCoord);
        }

        private bool TryEnqueueTap(Vector2Int sourceCoord)
        {
            if (!Context.GridModel.IsInRange(sourceCoord)) return false;

            var objA = Context.GridModel.GetGridObject(sourceCoord);
            if (!objA) return false;

            if (!IsInteractable(objA))
            {
                objA.Animation.Shake();
                return false;
            }

            var inputSource = new GridInputSource(GridInputType.Tap, sourceCoord);
            Context.Inputs.Enqueue(inputSource);
            return true;
        }

        private bool TryEnqueueSwap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (sourceCoord == targetCoord) return false;
            
            if (!Context.GridModel.IsInRange(sourceCoord)) return false;

            var sourceObj = Context.GridModel.GetGridObject(sourceCoord);
            var targetObj = Context.GridModel.GetGridObject(targetCoord);

            if (!sourceObj) return false;

            if (!targetObj || !Context.GridModel.IsInRange(targetCoord))
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
            Context.Inputs.Enqueue(inputSource);
            return true;
        }
        
        private static bool IsInteractable(BaseGridObject obj)
        {
            if (obj.IsNone) return false;
            return !obj.IsFallInProgress;
        }

        private static bool IsSwapCandidate(BaseGridObject obj) => !obj.IsStationary;
        public void Tick() => StateMachine.Update(Time.deltaTime);
        public void FixedTick() => StateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => StateMachine.LateUpdate(Time.deltaTime);
    }
}