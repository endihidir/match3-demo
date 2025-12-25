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
                EnqueueSingleBoosterEffect(a, booster.BoosterType);
                Context.CascadeResolveRequested = true;
            }

            Exit();
        }

        private void HandleSwap(in GridMove move)
        {
            var a = move.A;
            var b = move.B;

            var objA = Context.Model.GetGridObject(a);
            var objB = Context.Model.GetGridObject(b);

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
                PlaySwapAndCommit(objA, objB, a, b, false).Forget();
                return;
            }

            if (!GridMatchDetectUtil.IsRegularItem(objA.TypeData) ||
                !GridMatchDetectUtil.IsRegularItem(objB.TypeData) ||
                !WouldCreateMatchAfterSwap(a, b, objA.TypeData.TypeId, objB.TypeData.TypeId))
            {
                PlayPingPong(objA, objB, a, b).Forget();
                return;
            }

            PlaySwapAndCommit(objA, objB, a, b, true).Forget();
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

        private async UniTask PlaySwapAndCommit(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b, bool forceBoosterCenterToB)
        {
            var tasks = new List<UniTask>();

            var tweenA = objA.ItemAnimation.Move(Context.View.GridToWorld(b));
            var tweenB = objB.ItemAnimation.Move(Context.View.GridToWorld(a));

            tasks.Add(tweenA.AsyncWaitForCompletion().AsUniTask());
            tasks.Add(tweenB.AsyncWaitForCompletion().AsUniTask());

            await UniTask.WhenAll(tasks);

            Context.Model.Swap(a, b);

            if (forceBoosterCenterToB)
            {
                Context.IsForcedBoosterSpawnPos = true;
                Context.ForcedBoosterSpawnPos = b;
            }

            AfterSwapCommitted(a, b);

            Exit();
        }

        private void AfterSwapCommitted(Vector2Int a, Vector2Int b)
        {
            var objA = Context.Model.GetGridObject(a);
            var objB = Context.Model.GetGridObject(b);

            if (objA is BoosterObject boosterA && objB is BoosterObject boosterB)
            {
                EnqueueMergedEffects(b, boosterA.BoosterType, boosterB.BoosterType);
                Context.CascadeResolveRequested = true;
                return;
            }

            if (objB is BoosterObject movedBoosterToB)
            {
                EnqueueSingleBoosterEffect(b, movedBoosterToB.BoosterType);
                Context.CascadeResolveRequested = true;
                return;
            }

            if (objA is BoosterObject movedBoosterToA)
            {
                EnqueueSingleBoosterEffect(a, movedBoosterToA.BoosterType);
            }

            Context.CascadeResolveRequested = true;
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

        private bool WouldCreateMatchAfterSwap(Vector2Int a, Vector2Int b, int typeA, int typeB)
        {
            var width = Context.Model.Width;
            var height = Context.Model.Height;

            var grid = Context.Model.BuildTypeDataGrid();

            var cellA = grid[a.x, a.y];
            var cellB = grid[b.x, b.y];

            grid[a.x, a.y] = new GridObjectTypeData(cellA.ItemKind, typeB);
            grid[b.x, b.y] = new GridObjectTypeData(cellB.ItemKind, typeA);

            return GridMatchDetectUtil.WouldCreateBlastGroup(grid, a.x, a.y, width, height, typeB, false) ||
                   GridMatchDetectUtil.WouldCreateBlastGroup(grid, b.x, b.y, width, height, typeA, false);
        }

        protected override void OnExit()
        {
            RequestExit();
        }
    }
}