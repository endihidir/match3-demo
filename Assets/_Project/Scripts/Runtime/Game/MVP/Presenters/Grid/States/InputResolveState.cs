using System;
using Game.Grid.Contexts;
using Game.Grid.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Handlers.Data;
using Game.Level.Handlers;
using UnityEngine;

namespace Game.Grid.States
{
    public sealed class InputResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        private readonly IBoosterActionBuildHandler _boosterActionBuilder;
        private readonly ILevelGoalHandler _levelGoalHandler;

        public InputResolveState(GridStateContext context, IBoosterActionBuildHandler boosterActionBuilder, ILevelGoalHandler levelGoalHandler) : base(context)
        {
            _boosterActionBuilder = boosterActionBuilder;
            _levelGoalHandler = levelGoalHandler;
        }

        protected override void OnEnter()
        {
            if (Context.Inputs.Count == 0)
            {
                RequestExit();
                return;
            }

            var inputSource = Context.Inputs.Dequeue();

            var hasInputGet = inputSource.InputType switch
            {
                GridInputType.Tap => HandleTap(inputSource, RequestExit),
                GridInputType.Swap => HandleSwap(inputSource, RequestExit),
                _ => false
            };

            if (hasInputGet)
            {
                _levelGoalHandler.ProgressMove();
            }
            else
            {
                RequestExit();
            }
        }

        private bool HandleTap(in GridInputSource move, Action onComplete)
        {
            var sourceCoord = move.SourceCoord;
            var sourceObj = Context.GridModel.GetGridObject(sourceCoord);

            if (!sourceObj)
            {
                onComplete?.Invoke();
                return false;
            }

            if (sourceObj is not BoosterObject booster)
            {
                sourceObj.Animation.Shake();
                onComplete?.Invoke();
                return false;
            }

            _boosterActionBuilder.Build(booster, Context.PendingBoosterActions);
            onComplete?.Invoke();
            return true;
        }

        private bool HandleSwap(in GridInputSource move, Action onComplete)
        {
            var model = Context.GridModel;
            var sourceCoord = move.SourceCoord;
            var targetCoord = move.TargetCoord;

            var sourceObj = model.GetGridObject(sourceCoord);
            var targetObj = model.GetGridObject(targetCoord);

            if (!sourceObj || !targetObj)
            {
                onComplete?.Invoke();
                return false;
            }

            if (sourceObj.ObjectKind == GridObjectKind.Booster || targetObj.ObjectKind == GridObjectKind.Booster)
            {
                PlaySwapAndCommit(sourceObj, targetObj, onComplete).Forget();
                return true;
            }

            var typeData = model.BuildGridTypeData();

            if (!GridMatchCalcUtil.IsCellsRegular(sourceObj, targetObj) ||
                !GridMatchCalcUtil.WouldSwapCreateMatch(model, typeData, sourceCoord, targetCoord, sourceObj.TypeId, targetObj.TypeId))
            {
                PlaySwapAndBack(sourceObj, targetObj, sourceCoord, targetCoord, onComplete).Forget();
                return false;
            }

            PlaySwapAndCommit(sourceObj, targetObj, onComplete).Forget();
            return true;
        }

        private async UniTask PlaySwapAndBack(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord, Action onComplete)
        {
            var sourcePos = Context.GridView.GridToWorld(sourceCoord);
            var targetPos = Context.GridView.GridToWorld(targetCoord);

            _ = targetObj.Animation.PingPongMove(targetPos, sourcePos);
            await sourceObj.Animation.PingPongMove(sourcePos, targetPos);

            onComplete?.Invoke();
        }

        private async UniTask PlaySwapAndCommit(BaseGridObject sourceObj, BaseGridObject targetObj, Action onComplete)
        {
            var sourceCoord = sourceObj.Coord;
            var targetCoord = targetObj.Coord;

            var sourcePos = Context.GridView.GridToWorld(sourceCoord);
            var targetPos = Context.GridView.GridToWorld(targetCoord);

            sourceObj.SetFrontOf(targetObj);

            if (sourceObj.ObjectKind == GridObjectKind.Regular || targetObj.ObjectKind == GridObjectKind.Regular)
            {
                _ = targetObj.Animation.MoveTo(sourcePos);
            }

            await sourceObj.Animation.MoveTo(targetPos);

            Context.GridModel.Swap(sourceCoord, targetCoord);

            SetInputFlags(sourceObj, targetObj);
            BuildBoosterActions(sourceObj, targetObj);

            Context.MatchResolveRequested = true;
            onComplete?.Invoke();
        }

        private void SetInputFlags(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var sourceIsBooster = sourceObj.ObjectKind == GridObjectKind.Booster;
            var targetIsBooster = targetObj.ObjectKind == GridObjectKind.Booster;

            if (sourceIsBooster && targetIsBooster) return;
            
            Context.MergeCenterCoord = sourceObj.Coord;
            Context.ProtectedCoord = sourceIsBooster ? targetObj.Coord : targetIsBooster ? sourceObj.Coord : null;
        }

        private void BuildBoosterActions(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var actions = Context.PendingBoosterActions;

            if (sourceObj is BoosterObject source && targetObj is BoosterObject target)
            {
                _boosterActionBuilder.BuildCombo(source, target, actions);
                return;
            }

            if (targetObj is BoosterObject boosterB)
            {
                _boosterActionBuilder.Build(boosterB, actions);
                return;
            }

            if (sourceObj is BoosterObject boosterA)
            {
                _boosterActionBuilder.Build(boosterA, actions);
            }
        }
    }
}