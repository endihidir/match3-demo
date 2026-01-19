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

        public GridStateHandler(IGridModel model, IGridView view, IGridItemFactory factory, GameplayConfigContainer configContainer, IFillStrategyResolver fillStrategyResolver)
        {
            Context = new GridStateContext(model, view, factory, configContainer.ItemConfigContainer);

            var idleState = new IdleState(Context);
            var inputState = new InputResolveState(Context);
            var boosterState = new BoosterResolveState(Context);
            var matchState = new MatchResolveState(Context);
            var fillState = new FillResolveState(Context, fillStrategyResolver);

            var states = new StateBase<GridStateContext>[] { idleState, inputState, boosterState, matchState, fillState };
            
            StateMachine.Register(states);

            StateMachine.AddTransition(idleState, inputState, () => HasAnyInput)
                        .AddTransition(idleState, matchState, () => MatchResolveRequested)
                         
                        .AddTransition(inputState, boosterState, () => inputState.IsExitReady && HasPendingBoosterActions)
                        .AddTransition(inputState, matchState, () => inputState.IsExitReady && MatchResolveRequested && !HasPendingBoosterActions)
                        .AddTransition(inputState, idleState, () => inputState.IsExitReady && !MatchResolveRequested && !HasPendingBoosterActions)
                        
                        .AddTransition(boosterState, fillState, () => boosterState.IsExitReady)
                        .AddTransition(matchState, fillState, () => matchState.IsExitReady)
                        .AddTransition(fillState, idleState, () => fillState.IsExitReady);

            StateMachine.SetInitialState(idleState);
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
            if (!Context.Model.IsInRange(sourceCoord)) return false;

            var objA = Context.Model.GetGridObject(sourceCoord);
            if (!objA) return false;

            if (!IsInteractable(objA))
            {
                objA.ItemAnimation.Shake();
                return false;
            }

            var inputSource = new InputSource(GridInputType.Tap, sourceCoord);
            Context.Inputs.Enqueue(inputSource);
            return true;
        }

        private bool TryEnqueueSwap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (sourceCoord == targetCoord) return false;
            
            if (!Context.Model.IsInRange(sourceCoord)) return false;

            var sourceObj = Context.Model.GetGridObject(sourceCoord);
            var targetObj = Context.Model.GetGridObject(targetCoord);

            if (!sourceObj) return false;

            if (!targetObj || !Context.Model.IsInRange(targetCoord))
            {
                sourceObj.ItemAnimation.Shake();
                return false;
            }

            if (!IsInteractable(sourceObj) || !IsInteractable(targetObj) || 
                !IsSwapCandidate(sourceObj) || !IsSwapCandidate(targetObj))
            {
                sourceObj.ItemAnimation.Shake();
                return false;
            }

            Context.Inputs.Enqueue(new InputSource(GridInputType.Swap, sourceCoord, targetCoord));
            return true;
        }
        
        private static bool IsInteractable(BaseGridObject obj)
        {
            if (obj.IsEmpty) return false;
            return !obj.IsFallInProgress;
        }

        private static bool IsSwapCandidate(BaseGridObject obj) => !obj.IsStationary;
        public void Tick() => StateMachine.Update(Time.deltaTime);
        public void FixedTick() => StateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => StateMachine.LateUpdate(Time.deltaTime);
    }
}