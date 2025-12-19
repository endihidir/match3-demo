using System.Collections.Generic;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ExecuteMoveState : StateBase<GridContext>
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

            if (move.Type != GridMoveType.Swap)
            {
                Context.MoveQueue.Dequeue();
                Exit();
                return;
            }

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
                PlaySwapAndCommit(objA, objB, a, b).Forget();
                return;
            }

            if (!GridMatchDetectUtil.IsRegularItem(objA.TypeData) ||
                !GridMatchDetectUtil.IsRegularItem(objB.TypeData) || 
                !WouldCreateMatchAfterSwap(a, b, objA.TypeData.TypeId, objB.TypeData.TypeId))
            {
                PlayPingPong(objA, objB, a, b).Forget();
                return;
            }

            PlaySwapAndCommit(objA, objB, a, b).Forget();
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

        private async UniTask PlaySwapAndCommit(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b)
        {
            var tasks = new List<UniTask>();
            var tweenA = objA.ItemAnimation.Move(Context.View.GridToWorld(b));
            var tweenB = objB.ItemAnimation.Move(Context.View.GridToWorld(a));

            tasks.Add(tweenA.AsyncWaitForCompletion().AsUniTask());
            tasks.Add(tweenB.AsyncWaitForCompletion().AsUniTask());

            await UniTask.WhenAll(tasks);
            
            Context.Model.Swap(a, b);
            Exit();
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