using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ExecuteMoveState : StateBase<GridContext>
    {
        public override bool NeedsExitTime => true;

        protected override void OnInit() { }
        protected override bool OnBeforeEnter() => true;

        protected override void OnEnter()
        {
            if (Context.MoveQueue.Count == 0)
            {
                RequestExit();
                return;
            }

            var move = Context.MoveQueue.Dequeue();

            if (move.Type != GridMoveType.Swap)
            {
                RequestExit();
                return;
            }

            var a = move.A;
            var b = move.B;

            var objA = Context.Model.GetGridObject(a);
            var objB = Context.Model.GetGridObject(b);

            if (!objA || !objB)
            {
                RequestExit();
                return;
            }

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

        protected override void OnUpdate(float deltaTime) { }
        protected override void OnFixedUpdate(float deltaTime) { }
        protected override void OnLateUpdate(float deltaTime) { }
        protected override void OnExit() { }

        private void PlayPingPong(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b)
        {
            var aWorld = Context.View.GridToWorld(a);
            var bWorld = Context.View.GridToWorld(b);

            var animA = objA.ItemAnimation;
            var animB = objB.ItemAnimation;

            var completedCount = 0;

            animA.PingPongMove(bWorld, onComplete: OnOneDone);
            animB.PingPongMove(aWorld, onComplete: OnOneDone);
            return;

            void OnOneDone()
            {
                completedCount++;
                if (completedCount >= 2)
                    RequestExit();
            }
        }

        private void PlaySwapAndCommit(BaseItemObject objA, BaseItemObject objB, Vector2Int a, Vector2Int b)
        {
            var aWorld = Context.View.GridToWorld(a);
            var bWorld = Context.View.GridToWorld(b);

            var animA = objA.ItemAnimation;
            var animB = objB.ItemAnimation;

            var completedCount = 0;

            animA.Move(bWorld, onComplete: OnOneDone);
            animB.Move(aWorld, onComplete: OnOneDone);
            return;

            void OnOneDone()
            {
                completedCount++;
                if (completedCount < 2) return;

                Context.Model.Swap(a, b);
                RequestExit();
            }
        }

        private bool WouldCreateMatchAfterSwap(Vector2Int a, Vector2Int b, int typeA, int typeB)
        {
            var width = Context.Model.Width;
            var height = Context.Model.Height;

            var grid = BuildTypeGrid(width, height);

            var cellA = grid[a.x, a.y];
            var cellB = grid[b.x, b.y];

            grid[a.x, a.y] = new GridObjectTypeData(cellA.ItemKind, typeB);
            grid[b.x, b.y] = new GridObjectTypeData(cellB.ItemKind, typeA);

            var matchAtA = GridMatchDetectUtil.WouldCreateBlastGroup(grid, a.x, a.y, width, height, typeB, false);
            var matchAtB = GridMatchDetectUtil.WouldCreateBlastGroup(grid, b.x, b.y, width, height, typeA, false);

            return matchAtA || matchAtB;
        }

        private GridObjectTypeData[,] BuildTypeGrid(int width, int height)
        {
            var grid = new GridObjectTypeData[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var obj = Context.Model.GetGridObject(new Vector2Int(x, y));
                    grid[x, y] = obj ? obj.TypeData : default;
                }
            }

            return grid;
        }
    }
}