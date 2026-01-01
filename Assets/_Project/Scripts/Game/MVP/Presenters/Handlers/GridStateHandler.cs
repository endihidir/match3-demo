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
        public IStateMachine StateMachine { get; private set; }
        public GridStateContext Context { get; private set; }

        public GridStateHandler(IGridModel model, IGridView view, IGridItemFactory factory, GameplayConfigContainer configContainer, IRefillStrategyHandler strategyHandler)
        {
            StateMachine = new StateMachine();
            Context = new GridStateContext(model, view, factory, configContainer.ItemConfigContainerSo);

            var idleState = new IdleState(Context);
            var inputResolveState = new InputResolveState(Context);
            var boosterResolveState = new BoosterResolveState(Context);
            var matchResolveState = new MatchResolveState(Context);
            var refillResolveState = new RefillResolveState(strategyHandler, Context);

            var states = new StateBase<GridStateContext>[] { idleState, inputResolveState, boosterResolveState, matchResolveState, refillResolveState };
            StateMachine.Register(states);

            StateMachine.AddTransition(idleState, inputResolveState, () => Context.MoveQueue.Count > 0)
                         .AddTransition(idleState, matchResolveState, () => Context.RefillResolveRequested && !Context.RefillInProgress)
                         .AddTransition(idleState, refillResolveState, () => GridRefillCalcUtils.HasAnyEmptyActiveCell(Context.Model))
                         
                         .AddTransition(inputResolveState, boosterResolveState, () => inputResolveState.IsExitReady)
                         .AddTransition(inputResolveState, idleState, () => inputResolveState.IsExitReady)
                         
                         .AddTransition(boosterResolveState, refillResolveState, () => boosterResolveState.IsExitReady && Context.ResolvedAnyBooster)
                         .AddTransition(boosterResolveState, matchResolveState, () => boosterResolveState.IsExitReady && !Context.ResolvedAnyBooster)
                         
                         .AddTransition(matchResolveState, refillResolveState, () => matchResolveState.IsExitReady && Context.ResolvedAnyMatch)
                         .AddTransition(matchResolveState, idleState, () => matchResolveState.IsExitReady && !Context.ResolvedAnyMatch)
                         
                         .AddTransition(refillResolveState, idleState, () => refillResolveState.IsExitReady);

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

            Context.MoveQueue.Enqueue(new GridMove(GridMoveType.Tap, sourceCoord));
            return true;
        }

        private bool TryEnqueueSwap(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            if (sourceCoord == targetCoord) return false;
            
            if (!Context.Model.IsInRange(sourceCoord) || !Context.Model.IsInRange(targetCoord)) return false;

            var sourceObj = Context.Model.GetGridObject(sourceCoord);
            var targetObj = Context.Model.GetGridObject(targetCoord);

            if (!sourceObj || !targetObj) return false;

            if (!IsInteractable(sourceObj) || !IsInteractable(targetObj) || 
                !IsSwapCandidate(sourceObj) || !IsSwapCandidate(targetObj))
            {
                sourceObj.ItemAnimation.Shake();
                return false;
            }

            Context.MoveQueue.Enqueue(new GridMove(GridMoveType.Swap, sourceCoord, targetCoord));
            return true;
        }

        private static bool IsInteractable(BaseGridObject obj)
        {
            if (obj.IsEmpty) return false;
            return !obj.IsShiftInProgress;
        }

        private static bool IsSwapCandidate(BaseGridObject obj) => !obj.IsStationary;

        public void Tick() => StateMachine.Update(Time.deltaTime);
        public void FixedTick() => StateMachine.FixedUpdate(Time.fixedDeltaTime);
        public void LateTick() => StateMachine.LateUpdate(Time.deltaTime);
    }
}