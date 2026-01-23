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

            switch (inputSource.InputType)
            {
                case GridInputType.Tap:
                    HandleTap(inputSource);
                    return;
                case GridInputType.Swap:
                    HandleSwap(inputSource);
                    return;
                default:
                    RequestExit();
                    break;
            }
        }

        private void HandleTap(in GridInputSource move)
        {
            var sourceCoord = move.SourceCoord;
            
            var sourceObj = Context.Model.GetGridObject(sourceCoord);
            
            if (!sourceObj)
            {
                RequestExit();
                return;
            }

            if (sourceObj is not BoosterObject booster)
            {
                sourceObj.ItemAnimation.Shake();
                RequestExit();
                return;
            }

            AddBoosterAction(sourceCoord, booster);
            Context.ReleaseAndSetNull(sourceObj, sourceCoord);
            RequestExit();
        }

        private void HandleSwap(in GridInputSource move)
        {
            var model = Context.Model;
            var sourceCoord = move.SourceCoord;
            var targetCoord = move.TargetCoord;

            var sourceObj = model.GetGridObject(sourceCoord);
            var targetObj = model.GetGridObject(targetCoord);
         
            if (!sourceObj || !targetObj)
            {
                RequestExit();
                return;
            }

            if (sourceObj.ItemKind == GridItemKind.Booster || targetObj.ItemKind == GridItemKind.Booster)
            {
                PlaySwapAndCommit(sourceObj, targetObj).Forget();
                return;
            }

            var typeData = model.BuildGridTypeData();
            
            if (!GridMatchCalcUtil.IsCellsRegular(sourceObj, targetObj) || 
                !GridMatchCalcUtil.WouldSwapCreateMatch(model, typeData, sourceCoord, targetCoord, sourceObj.TypeId, targetObj.TypeId))
            {
                PlaySwapAndBack(sourceObj, targetObj, sourceCoord, targetCoord).Forget();
                return;
            }
            
            PlaySwapAndCommit(sourceObj, targetObj).Forget();
        }

        private async UniTask PlaySwapAndBack(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            var sourcePos = Context.View.GridToWorld(sourceCoord);
            var targetPos = Context.View.GridToWorld(targetCoord);
            
            _ = sourceObj.ItemAnimation.PingPongMove(sourcePos, targetPos);
            await targetObj.ItemAnimation.PingPongMove(targetPos, sourcePos);
           
            RequestExit();
        }

        private async UniTask PlaySwapAndCommit(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var sourceCoord = sourceObj.Coord;
            var targetCoord = targetObj.Coord;
            
            var sourcePos = Context.View.GridToWorld(sourceCoord);
            var targetPos = Context.View.GridToWorld(targetCoord);
            
            sourceObj.SetFrontOf(targetObj);
            
            if(sourceObj.ItemKind == GridItemKind.Regular || targetObj.ItemKind == GridItemKind.Regular)
            {
                _ = targetObj.ItemAnimation.MoveTo(sourcePos);
            }

            await sourceObj.ItemAnimation.MoveTo(targetPos);

            Context.Model.Swap(sourceCoord, targetCoord);
            
            SetInputBoosterFlagsAfterSwap(sourceObj, targetObj, sourceCoord, targetCoord);

            AfterSwapCommitted(sourceObj, targetObj);

            RequestExit();
        }
        
        private void SetInputBoosterFlagsAfterSwap(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            var sourceIsBooster = sourceObj.ItemKind == GridItemKind.Booster;
            var targetIsBooster = targetObj.ItemKind == GridItemKind.Booster;

            if (sourceIsBooster && targetIsBooster) return;

            Context.HasForcedBoosterSpawnCoord = true;
            Context.ForcedBoosterSpawnCoord = targetCoord;

            if (sourceIsBooster)
            {
                Context.HasInputTriggeredBooster = true;
                Context.InputTriggeredBoosterCoord = sourceCoord;
                return;
            }

            if (targetIsBooster)
            {
                Context.HasInputTriggeredBooster = true;
                Context.InputTriggeredBoosterCoord = targetCoord;
            }
        }

        private void AfterSwapCommitted(BaseGridObject sourceObj, BaseGridObject targetObj)
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
                Context.MatchResolveRequested = true;
                return;
            }

            if (sourceObj is BoosterObject movedBoosterToA)
            {
                AddBoosterAction(sourceCoord, movedBoosterToA);
                Context.ReleaseAndSetNull(sourceObj, sourceCoord);
            }
            
            Context.MatchResolveRequested = true;
        }

        private void AddBoosterAction(Vector2Int originCoord, BoosterObject booster)
        {
            if (!booster || booster.BoosterAction == null) return;

            var boosterActionContext = new BoosterActionContext(Context.NextBoosterGroupId(), originCoord, booster.BoosterAction);

            Context.PendingBoosterActions.Add(boosterActionContext);
        }

        private void AddComboAction(Vector2Int origin, BoosterType sourceBoosterType, BoosterType targetBoosterType)
        {
            var boosterComboConfig = Context.Configs.BoosterComboConfigSo;

            if (boosterComboConfig && boosterComboConfig.TryGetRule(sourceBoosterType, targetBoosterType, out var rule) && rule.Actions != null)
            {
                var nextGroupId = Context.NextBoosterGroupId();

                foreach (var boosterEffectBase in rule.Actions)
                {
                    if (boosterEffectBase == null) continue;

                    var boosterActionContext = new BoosterActionContext(nextGroupId, origin, boosterEffectBase);

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