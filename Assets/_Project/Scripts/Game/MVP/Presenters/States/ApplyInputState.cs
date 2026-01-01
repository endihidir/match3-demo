using System.Collections.Generic;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ApplyInputState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        protected override void OnEnter()
        {
            if (Context.MoveQueue.Count == 0)
            {
                RequestExit();
                return;
            }

            var move = Context.MoveQueue.Dequeue();

            switch (move.MoveType)
            {
                case GridMoveType.Tap:
                    HandleTap(move);
                    return;
                case GridMoveType.Swap:
                    HandleSwap(move);
                    return;
                default:
                    RequestExit();
                    break;
            }
        }

        private void HandleTap(in GridMove move)
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

            AddSingleBoosterEffect(sourceCoord, booster);
            Context.RefillResolveRequested = true;
            RequestExit();
        }

        private void HandleSwap(in GridMove move)
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
                PlaySwapAndCommit(sourceObj, targetObj, sourceCoord, targetCoord, false).Forget();
                return;
            }
            
            if (!GridMatchDetectUtil.IsRegularItem(sourceObj.ObjectType) || !GridMatchDetectUtil.IsRegularItem(targetObj.ObjectType) ||
                !IsCellsMatched(sourceCoord, targetCoord, sourceObj.ObjectType.TypeId, targetObj.ObjectType.TypeId))
            {
                PlaySwapAndBack(sourceObj, targetObj, sourceCoord, targetCoord).Forget();
                return;
            }
            
            PlaySwapAndCommit(sourceObj, targetObj, sourceCoord, targetCoord, true).Forget();
        }

        private async UniTask PlaySwapAndBack(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            var tasks = new List<UniTask>();

            var tweenA = sourceObj.ItemAnimation.PingPongMove(Context.View.GridToWorld(targetCoord));
            var tweenB = targetObj.ItemAnimation.PingPongMove(Context.View.GridToWorld(sourceCoord));

            tasks.Add(tweenA.AsyncWaitForCompletion().AsUniTask());
            tasks.Add(tweenB.AsyncWaitForCompletion().AsUniTask());

            await UniTask.WhenAll(tasks);

            RequestExit();
        }

        private async UniTask PlaySwapAndCommit(BaseGridObject sourceObj, BaseGridObject targetObj, Vector2Int sourceCoord, Vector2Int targetCoord, bool forceBoosterSpawnCoord)
        {
            var tasks = new List<UniTask>();

            var tweenA = sourceObj.ItemAnimation.Move(Context.View.GridToWorld(targetCoord));
            var tweenB = targetObj.ItemAnimation.Move(Context.View.GridToWorld(sourceCoord));

            tasks.Add(tweenA.AsyncWaitForCompletion().AsUniTask());
            tasks.Add(tweenB.AsyncWaitForCompletion().AsUniTask());

            await UniTask.WhenAll(tasks);

            Context.Model.Swap(sourceCoord, targetCoord);

            if (forceBoosterSpawnCoord)
            {
                Context.HasForcedBoosterSpawnCoord = true;
                Context.ForcedBoosterSpawnCoord = targetCoord;
            }

            AfterSwapCommitted(sourceCoord, targetCoord);

            RequestExit();
        }

        private void AfterSwapCommitted(Vector2Int sourceCoord, Vector2Int targetCoord)
        {
            var sourceObj = Context.Model.GetGridObject(sourceCoord);
            var targetObj = Context.Model.GetGridObject(targetCoord);

            if (sourceObj is BoosterObject sourceBooster && targetObj is BoosterObject targetBooster)
            {
                AddMergedEffects(targetCoord, sourceBooster.BoosterType, targetBooster.BoosterType);
                Context.RefillResolveRequested = true;
                return;
            }

            if (targetObj is BoosterObject movedBoosterToB)
            {
                AddSingleBoosterEffect(targetCoord, movedBoosterToB);
                Context.RefillResolveRequested = true;
                return;
            }

            if (sourceObj is BoosterObject movedBoosterToA)
            {
                AddSingleBoosterEffect(sourceCoord, movedBoosterToA);
            }

            Context.RefillResolveRequested = true;
        }

        private void AddSingleBoosterEffect(Vector2Int originCoord, BoosterObject booster)
        {
            if (!booster || booster.BoosterAction == null) return;

            Context.PendingBoosterActions.Add(new PendingBoosterAction(originCoord, booster.BoosterAction));
        }

        private void AddMergedEffects(Vector2Int origin, BoosterType sourceBoosterType, BoosterType targetBoosterType)
        {
            var boosterMergeConfig = Context.Configs.BoosterMergeConfigSo;

            if (boosterMergeConfig && boosterMergeConfig.TryGetRule(sourceBoosterType, targetBoosterType, out var rule) && rule.Actions != null)
            {
                for (int i = 0; i < rule.Actions.Length; i++)
                {
                    var boosterEffectBase = rule.Actions[i];
                    if (boosterEffectBase == null) continue;

                    Context.PendingBoosterActions.Add(new PendingBoosterAction(origin, boosterEffectBase));
                }

                return;
            }

            AddSingleBoosterEffect(origin, sourceBoosterType);
            AddSingleBoosterEffect(origin, targetBoosterType);
        }

        private void AddSingleBoosterEffect(Vector2Int originCoord, BoosterType type)
        {
            var configData = Context.Configs.GetBoosterData(type);

            if (!configData || configData.BoosterAction == null) return;

            Context.PendingBoosterActions.Add(new PendingBoosterAction(originCoord, configData.BoosterAction));
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