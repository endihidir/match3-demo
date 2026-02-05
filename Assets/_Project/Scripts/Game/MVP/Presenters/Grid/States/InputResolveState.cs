using System;
using Core.Config;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class InputResolveState : StateBase<GridStateContext>
    { 
        public override bool NeedsExitPermission => true;
        public InputResolveState(GridStateContext context) : base(context) { }

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
                Context.LevelObjectiveModel.ConsumeMove();
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

            AddBoosterAction(sourceCoord, booster);
            Context.ReleaseAndSetNull(sourceObj, sourceCoord);
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

            if (sourceObj.ItemKind == GridItemKind.Booster || targetObj.ItemKind == GridItemKind.Booster)
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
            
            _ = sourceObj.Animation.PingPongMove(sourcePos, targetPos);
            await targetObj.Animation.PingPongMove(targetPos, sourcePos);
           
            onComplete?.Invoke();
        }

        private async UniTask PlaySwapAndCommit(BaseGridObject sourceObj, BaseGridObject targetObj, Action onComplete)
        {
            var sourceCoord = sourceObj.Coord;
            var targetCoord = targetObj.Coord;
            
            var sourcePos = Context.GridView.GridToWorld(sourceCoord);
            var targetPos = Context.GridView.GridToWorld(targetCoord);
            
            sourceObj.SetFrontOf(targetObj);
            
            if(sourceObj.ItemKind == GridItemKind.Regular || targetObj.ItemKind == GridItemKind.Regular)
            {
                _ = targetObj.Animation.MoveTo(sourcePos);
            }

            await sourceObj.Animation.MoveTo(targetPos);

            Context.GridModel.Swap(sourceCoord, targetCoord);
            
            SetInputFlags(sourceObj, targetObj, sourceCoord, targetCoord);
            CreateBoosterActions(sourceObj, targetObj);
            
            Context.MatchResolveRequested = true;
            onComplete?.Invoke();
        }
        
        private void SetInputFlags(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            var sourceIsBooster = sourceObj.ItemKind == GridItemKind.Booster;
            var targetIsBooster = targetObj.ItemKind == GridItemKind.Booster;

            if (sourceIsBooster && targetIsBooster) return;

            Context.HasMergeCenterCoordRequested = true;
            Context.MergeCenterCoord = targetCoord;

            if (sourceIsBooster)
            {
                Context.HasUnmarkRemoveRequested = true;
                Context.UnmarkRemoveCoord = sourceCoord;
                return;
            }

            if (targetIsBooster)
            {
                Context.HasUnmarkRemoveRequested = true;
                Context.UnmarkRemoveCoord = targetCoord;
            }
        }

        private void CreateBoosterActions(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var sourceCoord = sourceObj.Coord;
            var targetCoord = targetObj.Coord;
            
            if (sourceObj is BoosterObject sourceBooster && targetObj is BoosterObject targetBooster)
            {
                AddComboAction(sourceCoord, sourceBooster.BoosterType, targetBooster.BoosterType);
                Context.ReleaseAndSetNull(sourceObj, sourceCoord);
                Context.ReleaseAndSetNull(targetObj, targetCoord);
                return;
            }

            if (targetObj is BoosterObject movedBoosterToB)
            {
                AddBoosterAction(targetCoord, movedBoosterToB);
                Context.ReleaseAndSetNull(targetObj, targetCoord);
                return;
            }

            if (sourceObj is BoosterObject movedBoosterToA)
            {
                AddBoosterAction(sourceCoord, movedBoosterToA);
                Context.ReleaseAndSetNull(sourceObj, sourceCoord);
            }
        }

        private void AddBoosterAction(Vector2Int originCoord, BoosterObject booster)
        {
            if (!booster || booster.BoosterAction == null) return;

            var boosterActionContext = new BoosterActionContext(Context.NextBoosterGroupId(), originCoord, booster.BoosterAction);

            Context.PendingBoosterActions.Add(boosterActionContext);
        }

        private void AddComboAction(Vector2Int origin, BoosterType sourceBoosterType, BoosterType targetBoosterType)
        {
            var boosterComboConfig = Context.GridConfigs.GetConfig<BoosterConfigContainerSO>().BoosterComboConfigSo;

            if (boosterComboConfig && boosterComboConfig.TryGetRule(sourceBoosterType, targetBoosterType, out var rule) && rule.Actions != null)
            {
                var nextGroupId = Context.NextBoosterGroupId();

                foreach (var boosterAction in rule.Actions)
                {
                    if (boosterAction == null) continue;

                    var boosterActionContext = new BoosterActionContext(nextGroupId, origin, boosterAction);

                    Context.PendingBoosterActions.Add(boosterActionContext);
                }
            }
            else
            {
                EditorLogger.LogError($"{sourceBoosterType} - {targetBoosterType} merge rule does not exist!");
            }
        }
    }
}