using System;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ShuffleState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        private UniTask[] _animTasks = Array.Empty<UniTask>();
        private Vector2Int[] _coords = Array.Empty<Vector2Int>();
        private BaseGridObject[] _objs = Array.Empty<BaseGridObject>();

        private const int MaxMatchBreakOps = 64;

        public ShuffleState(GridStateContext context) : base(context) { }

        protected override void OnEnter()
        {
            if (HasAnyMove())
            {
                RequestExit();
                return;
            }

            ShuffleAsync().Forget();
        }

        private async UniTask ShuffleAsync()
        {
            Context.IsShuffleInProgress = true;

            try
            {
                if (HasAnyMove())
                {
                    RequestExit();
                    return;
                }

                var count = CollectShuffleCandidates();

                if (count < 2)
                {
                    RequestExit();
                    return;
                }

                ShuffleOnce(count);

                BreakExistingMatches();

                if (!HasAnyMove())
                    ForceCreateAnyMove_NoTypeChange();

                BreakExistingMatches();

                EnsureTaskCapacity(count);

                for (int i = 0; i < count; i++)
                {
                    var obj = _objs[i];
                    if (!obj)
                    {
                        _animTasks[i] = UniTask.CompletedTask;
                        continue;
                    }

                    var targetPos = Context.View.GridToWorld(obj.Coord);
                    var tween = obj.ItemAnimation.MoveTo(targetPos, 5f, Ease.InOutQuad);

                    _animTasks[i] = tween?.ToUniTask() ?? UniTask.CompletedTask;
                }

                if (count > 0)
                    await UniTask.WhenAll(_animTasks);
            }
            finally
            {
                Context.IsShuffleInProgress = false;
            }

            RequestExit();
        }

        private int CollectShuffleCandidates()
        {
            var model = Context.Model;
            var capacity = model.Width * model.Height;

            if (_coords.Length < capacity)
                _coords = new Vector2Int[capacity];

            if (_objs.Length < capacity)
                _objs = new BaseGridObject[capacity];

            var count = 0;

            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var obj = model.GetGridObject(x, y);
                    if (!IsShuffleCandidate(obj)) continue;

                    _coords[count] = new Vector2Int(x, y);
                    _objs[count] = obj;
                    count++;
                }
            }

            return count;
        }

        private void ShuffleOnce(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var j = UnityEngine.Random.Range(i, count);
                (_objs[i], _objs[j]) = (_objs[j], _objs[i]);
            }

            var model = Context.Model;

            for (int i = 0; i < count; i++)
                model.SetGridObject(_coords[i], _objs[i]);
        }

        public bool HasAnyMove()
        {
            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();

            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var a = model.GetGridObject(x, y);
                    if (!IsSwapCandidate(a)) continue;

                    if (TrySwapCreatesMatch(grid, x, y, x + 1, y)) return true;
                    if (TrySwapCreatesMatch(grid, x, y, x, y + 1)) return true;
                }
            }

            return false;
        }

        private bool TrySwapCreatesMatch(GridObjectType[,] grid, int ax, int ay, int bx, int by)
        {
            var model = Context.Model;

            if (!model.IsInRange(bx, by)) return false;

            var objA = model.GetGridObject(ax, ay);
            var objB = model.GetGridObject(bx, by);

            if (!IsSwapCandidate(objA) || !IsSwapCandidate(objB)) return false;

            if (!GridMatchCalc.IsCellsRegular(objA, objB)) return false;

            var cellA = grid[ax, ay];
            var cellB = grid[bx, by];

            grid[ax, ay] = new GridObjectType(cellA.ItemKind, cellB.TypeId);
            grid[bx, by] = new GridObjectType(cellB.ItemKind, cellA.TypeId);

            var matched = GridMatchCalc.IsCellMatched(model, grid, ax, ay, cellB.TypeId) ||
                          GridMatchCalc.IsCellMatched(model, grid, bx, by, cellA.TypeId);

            grid[ax, ay] = cellA;
            grid[bx, by] = cellB;

            return matched;
        }

        private void BreakExistingMatches()
        {
            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();

            for (int op = 0; op < MaxMatchBreakOps; op++)
            {
                if (!TryFindAnyMatchedCell(grid, out var matchCoord))
                    return;

                if (!TryBreakMatchAt(grid, matchCoord))
                    return;
            }
        }

        private bool TryFindAnyMatchedCell(GridObjectType[,] grid, out Vector2Int coord)
        {
            var model = Context.Model;

            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var obj = model.GetGridObject(x, y);
                    if (!IsShuffleCandidate(obj)) continue;

                    var typeId = obj.TypeId;

                    if (GridMatchCalc.IsCellMatched(model, grid, x, y, typeId))
                    {
                        coord = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            coord = default;
            return false;
        }

        private bool TryBreakMatchAt(GridObjectType[,] grid, Vector2Int matchCoord)
        {
            var model = Context.Model;

            var ax = matchCoord.x;
            var ay = matchCoord.y;

            var objA = model.GetGridObject(ax, ay);
            if (!IsShuffleCandidate(objA))
                return false;

            var cellA = grid[ax, ay];

            for (int i = 0; i < _coords.Length; i++)
            {
                var bCoord = _coords[i];

                if (bCoord == matchCoord)
                    continue;

                if (!model.IsInRange(bCoord)) 
                    continue;

                if (!model.IsCellActive(bCoord))
                    continue;

                var bx = bCoord.x;
                var by = bCoord.y;

                var objB = model.GetGridObject(bx, by);
                if (!IsShuffleCandidate(objB))
                    continue;

                var cellB = grid[bx, by];

                if (cellA.TypeId == cellB.TypeId)
                    continue;

                if (!GridMatchCalc.IsCellsRegular(objA, objB))
                    continue;

                if (WouldCreateMatchAfterSwap(grid, ax, ay, bx, by))
                    continue;

                model.Swap(matchCoord, bCoord);
                SwapGridCells(grid, ax, ay, bx, by);
                return true;
            }

            return false;
        }

        private bool WouldCreateMatchAfterSwap(GridObjectType[,] grid, int ax, int ay, int bx, int by)
        {
            var model = Context.Model;

            var cellA = grid[ax, ay];
            var cellB = grid[bx, by];

            grid[ax, ay] = new GridObjectType(cellA.ItemKind, cellB.TypeId);
            grid[bx, by] = new GridObjectType(cellB.ItemKind, cellA.TypeId);

            var creates = GridMatchCalc.IsCellMatched(model, grid, ax, ay, cellB.TypeId) ||
                          GridMatchCalc.IsCellMatched(model, grid, bx, by, cellA.TypeId);

            grid[ax, ay] = cellA;
            grid[bx, by] = cellB;

            return creates;
        }

        private void SwapGridCells(GridObjectType[,] grid, int ax, int ay, int bx, int by) => (grid[ax, ay], grid[bx, by]) = (grid[bx, by], grid[ax, ay]);

        private void ForceCreateAnyMove_NoTypeChange()
        {
            var model = Context.Model;

            for (int i = 0; i < _objs.Length; i++)
            {
                var a = _objs[i];
                if (!IsShuffleCandidate(a)) continue;

                var typeId = a.TypeId;

                BaseGridObject b = null;
                BaseGridObject c = null;

                for (int j = i + 1; j < _objs.Length; j++)
                {
                    var o = _objs[j];
                    if (!IsShuffleCandidate(o)) continue;
                    if (o.TypeId != typeId) continue;

                    if (b == null) { b = o; continue; }
                    c = o;
                    break;
                }

                if (!b || !c) continue;

                for (int y = 0; y < model.Height - 2; y++)
                {
                    for (int x = 0; x < model.Width - 1; x++)
                    {
                        var p0 = new Vector2Int(x, y);
                        var p1 = new Vector2Int(x, y + 1);
                        var p2 = new Vector2Int(x + 1, y + 2);
                        var pGap = new Vector2Int(x, y + 2);

                        if (!IsCellUsableForMove(p0) || !IsCellUsableForMove(p1) || !IsCellUsableForMove(p2) || !IsCellUsableForMove(pGap))
                            continue;

                        var gapObj = model.GetGridObject(pGap);
                        if (gapObj && gapObj.TypeId == typeId)
                            continue;

                        PlaceObjectAt(a, p0);
                        PlaceObjectAt(b, p1);
                        PlaceObjectAt(c, p2);

                        return;
                    }
                }
            }
        }

        private bool IsCellUsableForMove(Vector2Int coord)
        {
            var model = Context.Model;

            if (!model.IsInRange(coord)) return false;
            if (!model.IsCellActive(coord)) return false;

            var obj = model.GetGridObject(coord);
            return IsShuffleCandidate(obj);
        }

        private void PlaceObjectAt(BaseGridObject obj, Vector2Int targetCoord)
        {
            if (!obj) return;

            var model = Context.Model;

            if (obj.Coord == targetCoord)
                return;

            model.Swap(obj.Coord, targetCoord);
        }

        private void EnsureTaskCapacity(int count)
        {
            if (_animTasks.Length != count)
                Array.Resize(ref _animTasks, count);
        }

        private static bool IsShuffleCandidate(BaseGridObject obj)
        {
            if (!obj) return false;
            if (obj.IsEmpty) return false;
            if (obj.IsStationary) return false;
            if (obj is BoosterObject) return false;
            return true;
        }

        private static bool IsSwapCandidate(BaseGridObject obj) => IsShuffleCandidate(obj);
    }
}