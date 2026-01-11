using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

        private void HandleTap(in InputSource move)
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
            ReleaseItem(sourceObj, sourceCoord);
            RequestExit();
        }

        private void HandleSwap(in InputSource move)
        {
            var sourceCoord = move.SourceCoord;
            var targetCoord = move.TargetCoord;

            var sourceObj = Context.Model.GetGridObject(sourceCoord);
            var targetObj = Context.Model.GetGridObject(targetCoord);
         
            if (!sourceObj || !targetObj)
            {
                RequestExit();
                return;
            }
           
            if (sourceObj is BoosterObject || targetObj is BoosterObject)
            {
                PlaySwapAndCommit(sourceObj, targetObj, false).Forget();
                return;
            }
            
            if (!IsCellsRegular(sourceObj, targetObj) || !IsCellsMatched(sourceCoord, targetCoord, sourceObj.ObjectType.TypeId, targetObj.ObjectType.TypeId))
            {
                PlaySwapAndBack(sourceObj, targetObj, sourceCoord, targetCoord).Forget();
                return;
            }
            
            PlaySwapAndCommit(sourceObj, targetObj, true).Forget();
        }

        private async UniTask PlaySwapAndBack(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            var sourcePos = Context.View.GridToWorld(sourceCoord);
            var targetPos = Context.View.GridToWorld(targetCoord);
            
            sourceObj.ItemAnimation.PingPongMove(targetPos);
            var tween = targetObj.ItemAnimation.PingPongMove(sourcePos);
            await tween.AsyncWaitForCompletion();
            RequestExit();
        }

        private async UniTask PlaySwapAndCommit(BaseGridObject sourceObj, BaseGridObject targetObj, bool forceBoosterSpawnCoord)
        {
            var sourceCoord = sourceObj.Coord;
            var targetCoord = targetObj.Coord;
            
            var sourcePos = Context.View.GridToWorld(sourceCoord);
            var targetPos = Context.View.GridToWorld(targetCoord);
            
            sourceObj.ItemAnimation.Move(targetPos);
            var tween = targetObj.ItemAnimation.Move(sourcePos);
            await tween.AsyncWaitForCompletion();
            
            Context.Model.Swap(sourceCoord, targetCoord);

            if (forceBoosterSpawnCoord)
            {
                Context.HasForcedBoosterSpawnCoord = true;
                Context.ForcedBoosterSpawnCoord = targetCoord;
            }

            AfterSwapCommitted(sourceObj, targetObj);

            RequestExit();
        }

        private void AfterSwapCommitted(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var sourceCoord = sourceObj.Coord;
            var targetCoord = targetObj.Coord;
            
            if (sourceObj is BoosterObject sourceBooster && targetObj is BoosterObject targetBooster)
            {
                AddComboAction(sourceCoord, sourceBooster.BoosterType, targetBooster.BoosterType);
                ReleaseItem(sourceObj, sourceCoord);
                ReleaseItem(targetObj, targetCoord);
                return;
            }

            if (targetObj is BoosterObject movedBoosterToB)
            {
                AddBoosterAction(targetCoord, movedBoosterToB);
                ReleaseItem(targetObj, targetCoord);
                return;
            }

            if (sourceObj is BoosterObject movedBoosterToA)
            {
                AddBoosterAction(sourceCoord, movedBoosterToA);
                ReleaseItem(sourceObj, sourceCoord);
                return;
            }
            
            Context.MatchResolveRequested = true;
        }

        private void AddBoosterAction(Vector2Int originCoord, BoosterObject booster)
        {
            if (!booster || booster.BoosterAction == null) return;

            Context.PendingBoosterActions.Add(new BoosterActionContext(Context.NextBoosterGroupId(), originCoord, booster.BoosterAction));
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

                    Context.PendingBoosterActions.Add(new BoosterActionContext(nextGroupId, origin, boosterEffectBase));
                }
            }
            else
            {
                EditorLogger.LogError($"{sourceBoosterType} - {targetBoosterType} merge rule does not exist!");
            }
        }
        
        private void ReleaseItem(BaseGridObject sourceObj, Vector2Int sourceCoord)
        {
            Context.Factory.ReleaseItem(sourceObj);
            Context.Model.SetGridObject(sourceCoord, null);
        }

        private bool IsCellsRegular(BaseGridObject sourceObj, BaseGridObject targetObj)
        {
            var isSourceRegular = GridMatchDetectUtil.IsRegularItem(sourceObj.ObjectType);
            var isTargetRegular = GridMatchDetectUtil.IsRegularItem(targetObj.ObjectType);
            return isSourceRegular && isTargetRegular;
        }
        
        private bool IsCellsMatched(Vector2Int coordA, Vector2Int coordB, int typeA, int typeB)
        {
            var model = Context.Model;

            var grid = model.BuildTypeDataGrid();

            var cellA = grid[coordA.x, coordA.y];
            var cellB = grid[coordB.x, coordB.y];

            grid[coordA.x, coordA.y] = new GridObjectType(cellA.ItemKind, typeB);
            grid[coordB.x, coordB.y] = new GridObjectType(cellB.ItemKind, typeA);

            return GridMatchRules.IsCellMatched(model, grid, coordA.x, coordA.y, typeB) ||
                   GridMatchRules.IsCellMatched(model, grid, coordB.x, coordB.y, typeA);
        }
    }
}