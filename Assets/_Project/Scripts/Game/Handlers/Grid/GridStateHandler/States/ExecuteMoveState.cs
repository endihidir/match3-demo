using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ExecuteMoveState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;
        public bool IsExitReady { get; private set; }
        public bool ResolveRequested { get; private set; }

        protected override void OnEnter()
        {
            IsExitReady = false;
            ResolveRequested = false;

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

            if (Context.MovingCells.Contains(a) || Context.MovingCells.Contains(b))
            {
                Exit();
                return;
            }

            var objA = Context.Model.GetGridObject(a);
            var objB = Context.Model.GetGridObject(b);

            if (!objA || !objB)
            {
                Context.MoveQueue.Dequeue();
                Exit();
                return;
            }

            if (objA.ItemAnimation.IsShiftInProgress || objB.ItemAnimation.IsShiftInProgress)
            {
                Exit();
                return;
            }

            Context.MoveQueue.Dequeue();

            if (objA is BoosterObject || objB is BoosterObject)
            {
                PlaySwapAndCommit(objA, objB, a, b);
                return;
            }

            if (!GridMatchDetectUtil.IsRegularItem(objA.TypeData) ||
                !GridMatchDetectUtil.IsRegularItem(objB.TypeData))
            {
                PlayPingPong(objA, objB, a, b);
                return;
            }

            if (!WouldCreateMatchAfterSwap(a, b, objA.TypeData.TypeId, objB.TypeData.TypeId))
            {
                PlayPingPong(objA, objB, a, b);
                return;
            }

            PlaySwapAndCommit(objA, objB, a, b);
        }

        private void PlayPingPong(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b)
        {
            Context.MovingCells.Add(a);
            Context.MovingCells.Add(b);

            var completed = 0;

            var tweenA = objA.ItemAnimation.PingPongMove(Context.View.GridToWorld(b), onComplete: OnDone);
            var tweenB = objB.ItemAnimation.PingPongMove(Context.View.GridToWorld(a), onComplete: OnDone);

            if (tweenA == null) OnDone();
            if (tweenB == null) OnDone();
            return;

            void OnDone()
            {
                completed++;
                if (completed < 2) return;

                Context.MovingCells.Remove(a);
                Context.MovingCells.Remove(b);
                Exit();
            }
        }

        private void PlaySwapAndCommit(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b)
        {
            Context.MovingCells.Add(a);
            Context.MovingCells.Add(b);

            var completed = 0;

            var tweenA = objA.ItemAnimation.Move(Context.View.GridToWorld(b), onComplete: OnDone);
            var tweenB = objB.ItemAnimation.Move(Context.View.GridToWorld(a), onComplete: OnDone);

            if (tweenA == null) OnDone();
            if (tweenB == null) OnDone();
            return;

            void OnDone()
            {
                completed++;
                if (completed < 2) return;

                Context.Model.Swap(a, b);
                ResolveRequested = true;
                Context.MovingCells.Remove(a);
                Context.MovingCells.Remove(b);
                Exit();
            }
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

            return
                GridMatchDetectUtil.WouldCreateBlastGroup(grid, a.x, a.y, width, height, typeB, false) ||
                GridMatchDetectUtil.WouldCreateBlastGroup(grid, b.x, b.y, width, height, typeA, false);
        }
        
        protected override void OnExit()
        {
            IsExitReady = true;
            RequestExit();
        }
        
        protected override void OnUpdate(float deltaTime) { }
        protected override void OnFixedUpdate(float deltaTime) { }
        protected override void OnLateUpdate(float deltaTime) { }
        protected override void OnInit() { }
        protected override bool OnBeforeEnter() => true;
    }
}