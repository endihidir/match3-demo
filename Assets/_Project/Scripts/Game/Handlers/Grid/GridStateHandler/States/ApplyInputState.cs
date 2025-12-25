using System.Collections.Generic;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ApplyInputState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        protected override void OnEnter()
        {
            if (Context.MoveQueue.Count == 0)
            {
                Exit();
                return;
            }

            var move = Context.MoveQueue.Peek();

            if (move.Type == GridMoveType.Tap)
            {
                HandleBoosterTap(move);
                return;
            }

            if (move.Type != GridMoveType.Swap)
            {
                Context.MoveQueue.Dequeue();
                Exit();
                return;
            }

            HandleSwap(move);
        }

        private void HandleBoosterTap(in GridMove move)
        {
            var a = move.A;

            var obj = Context.Model.GetGridObject(a);
            if (!obj)
            {
                Context.MoveQueue.Dequeue();
                Exit();
                return;
            }

            if (obj.IsShiftInProgress)
            {
                Exit();
                return;
            }

            Context.MoveQueue.Dequeue();

            if (obj is BoosterObject booster)
            {
                EnqueueSingleBoosterEffect(a, booster);
                Context.CascadeResolveRequested = true;
            }

            Exit();
        }

        private void HandleSwap(in GridMove move)
        {
            var coordA = move.A;
            var coordB = move.B;

            var objA = Context.Model.GetGridObject(coordA);
            var objB = Context.Model.GetGridObject(coordB);

            if (!objA || !objB)
            {
                Context.MoveQueue.Dequeue();
                Exit();
                return;
            }

            if (objA.IsShiftInProgress || objB.IsShiftInProgress)
            {
                Exit();
                return;
            }

            Context.MoveQueue.Dequeue();

            if (objA is BoosterObject || objB is BoosterObject)
            {
                PlaySwapAndCommit(objA, objB, coordA, coordB, false).Forget();
                return;
            }

            if (!GridMatchDetectUtil.IsRegularItem(objA.TypeData) ||
                !GridMatchDetectUtil.IsRegularItem(objB.TypeData) ||
                !WouldCreateMatchAfterSwap(coordA, coordB, objA.TypeData.TypeId, objB.TypeData.TypeId))
            {
                PlayPingPong(objA, objB, coordA, coordB).Forget();
                return;
            }

            PlaySwapAndCommit(objA, objB, coordA, coordB, true).Forget();
        }

        private async UniTask PlayPingPong(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b)
        {
            var tasks = new List<UniTask>();

            var tweenA = objA.ItemAnimation.PingPongMove(Context.View.GridToWorld(b));
            var tweenB = objB.ItemAnimation.PingPongMove(Context.View.GridToWorld(a));

            tasks.Add(tweenA.AsyncWaitForCompletion().AsUniTask());
            tasks.Add(tweenB.AsyncWaitForCompletion().AsUniTask());

            await UniTask.WhenAll(tasks);

            Exit();
        }

        private async UniTask PlaySwapAndCommit(BaseItemObject objA, BaseItemObject objB, Vector2Int coordA, Vector2Int coordB, bool forceBoosterCenterToB)
        {
            var tasks = new List<UniTask>();

            var tweenA = objA.ItemAnimation.Move(Context.View.GridToWorld(coordB));
            var tweenB = objB.ItemAnimation.Move(Context.View.GridToWorld(coordA));

            tasks.Add(tweenA.AsyncWaitForCompletion().AsUniTask());
            tasks.Add(tweenB.AsyncWaitForCompletion().AsUniTask());

            await UniTask.WhenAll(tasks);

            Context.Model.Swap(coordA, coordB);

            if (forceBoosterCenterToB)
            {
                Context.IsForcedBoosterSpawnPos = true;
                Context.ForcedBoosterSpawnPos = coordB;
            }

            AfterSwapCommitted(coordA, coordB);

            Exit();
        }

        private void AfterSwapCommitted(Vector2Int coordA, Vector2Int coordB)
        {
            var objA = Context.Model.GetGridObject(coordA);
            var objB = Context.Model.GetGridObject(coordB);

            if (objA is BoosterObject boosterA && objB is BoosterObject boosterB)
            {
                EnqueueMergedEffects(coordB, boosterA.BoosterType, boosterB.BoosterType);
                Context.CascadeResolveRequested = true;
                return;
            }

            if (objB is BoosterObject movedBoosterToB)
            {
                EnqueueSingleBoosterEffect(coordB, movedBoosterToB);
                Context.CascadeResolveRequested = true;
                return;
            }

            if (objA is BoosterObject movedBoosterToA)
            {
                EnqueueSingleBoosterEffect(coordA, movedBoosterToA);
            }

            Context.CascadeResolveRequested = true;
        }

        private void EnqueueSingleBoosterEffect(Vector2Int origin, BoosterObject booster)
        {
            if (!booster || booster.BoosterEffect == null) return;

            Context.PendingEffects.Add(new PendingEffect(origin, booster.BoosterEffect));
        }

        private void EnqueueSingleBoosterEffect(Vector2Int origin, BoosterType type)
        {
            var data = Context.Configs.GetBoosterData(type);

            if (!data || data.BoosterEffect == null) return;

            Context.PendingEffects.Add(new PendingEffect(origin, data.BoosterEffect));
        }

        private void EnqueueMergedEffects(Vector2Int origin, BoosterType first, BoosterType second)
        {
            var config = Context.Configs.BoosterMergeConfig;

            if (config && config.TryGetRule(first, second, out var rule) && rule.Effects != null)
            {
                for (int i = 0; i < rule.Effects.Length; i++)
                {
                    var e = rule.Effects[i];
                    if (e == null) continue;

                    Context.PendingEffects.Add(new PendingEffect(origin, e));
                }

                return;
            }

            EnqueueSingleBoosterEffect(origin, first);
            EnqueueSingleBoosterEffect(origin, second);
        }

        private bool WouldCreateMatchAfterSwap(Vector2Int coordA, Vector2Int coordB, int typeA, int typeB)
        {
            var width = Context.Model.Width;
            var height = Context.Model.Height;

            var grid = Context.Model.BuildTypeDataGrid();

            var cellA = grid[coordA.x, coordA.y];
            var cellB = grid[coordB.x, coordB.y];

            grid[coordA.x, coordA.y] = new GridObjectTypeData(cellA.ItemKind, typeB);
            grid[coordB.x, coordB.y] = new GridObjectTypeData(cellB.ItemKind, typeA);

            return GridMatchDetectUtil.WouldCreateBlastGroup(grid, coordA.x, coordA.y, width, height, typeB, false) ||
                   GridMatchDetectUtil.WouldCreateBlastGroup(grid, coordB.x, coordB.y, width, height, typeA, false);
        }

        protected override void OnExit()
        {
            RequestExit();
        }
    }
}
