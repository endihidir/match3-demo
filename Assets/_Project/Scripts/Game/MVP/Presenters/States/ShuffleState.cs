using System;
using Core.Item;
using Core.Models;
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
        public bool IsShuffleInProgress { get; private set; }

        private UniTask[] _animTasks = Array.Empty<UniTask>();
        private Vector2Int[] _coords = Array.Empty<Vector2Int>();
        private BaseGridObject[] _objs = Array.Empty<BaseGridObject>();
        private int[] _typeCounts = Array.Empty<int>();
        private int _candidateCount;
        private const int MaxMatchBreakOps = 64;
        
        public ShuffleState(GridStateContext context) : base(context) { }

        protected override void OnEnter()
        {
            var model = Context.Model;
            var grid = model.BuildTypeDataGrid();

            if (HasAnyMove(model, grid))
            {
                RequestExit();
                return;
            }

            ShuffleAsync().Forget();
        }

        private async UniTask ShuffleAsync()
        {
            IsShuffleInProgress = true;

            try
            {
                var model = Context.Model;

                var count = CollectShuffleCandidates();
                
                _candidateCount = count;

                if (count < 2) return;

                ShuffleOnce(count);
                
                var grid = model.BuildTypeDataGrid();

                BreakExistingMatches(model, grid);

                if (!HasAnyMove(model, grid))
                    ForceCreateAnyMove_NoTypeChange(model, grid);

                BreakExistingMatches(model, grid);

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
                IsShuffleInProgress = false;
                RequestExit();
            }

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
                    if (!GridShuffleCalcUtil.IsSwapCandidate(obj)) continue;

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
            return HasAnyMove(model, grid);
        }

        private bool HasAnyMove(IGridModel model, GridObjectType[,] grid)
        {
            if (!GridShuffleCalcUtil.HasAnyPotentialMatchGeometry(model)) return true;
            
            if (!CanEverFormAnyMatch(model)) return true;
            
            if (!GridShuffleCalcUtil.HasAnySwappableAdjacency(model)) return true;

            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var a = model.GetGridObject(x, y);
                    if (!GridShuffleCalcUtil.IsSwapCandidate(a)) continue;

                    if (GridShuffleCalcUtil.TrySwapCreatesMatch(model, grid, x, y, x + 1, y)) return true;
                    if (GridShuffleCalcUtil.TrySwapCreatesMatch(model, grid, x, y, x, y + 1)) return true;
                }
            }

            return false;
        }

        private bool CanEverFormAnyMatch(IGridModel model)
        {
            var maxTypeId = -1;

            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var obj = model.GetGridObject(x, y);
                    if (!GridShuffleCalcUtil.IsSwapCandidate(obj)) continue;
                    if (obj.TypeId > maxTypeId)
                        maxTypeId = obj.TypeId;
                }
            }

            if (maxTypeId < 0)
                return false;

            var required = maxTypeId + 1;
            if (_typeCounts.Length < required)
                _typeCounts = new int[required];
            else
                Array.Clear(_typeCounts, 0, required);

            for (int y = 0; y < model.Height; y++)
            {
                for (int x = 0; x < model.Width; x++)
                {
                    var obj = model.GetGridObject(x, y);
                    if (!GridShuffleCalcUtil.IsSwapCandidate(obj)) continue;

                    var typeId = obj.TypeId;
                    if ((uint)typeId >= (uint)required) continue;

                    _typeCounts[typeId]++;
                }
            }

            for (int i = 0; i < required; i++)
            {
                if (_typeCounts[i] >= 3)
                    return true;
            }

            return false;
        }

        private void BreakExistingMatches(IGridModel model, GridObjectType[,] grid)
        {
            for (int op = 0; op < MaxMatchBreakOps; op++)
            {
                if (!GridShuffleCalcUtil.TryFindAnyMatchedCell(model, grid, out var matchCoord))
                    return;

                if (!TryBreakMatchAt(model, grid, matchCoord))
                    return;
            }
        }

        private bool TryBreakMatchAt(IGridModel model, GridObjectType[,] grid, Vector2Int matchCoord)
        {
            var ax = matchCoord.x;
            var ay = matchCoord.y;

            var objA = model.GetGridObject(ax, ay);
            if (!GridShuffleCalcUtil.IsSwapCandidate(objA))
                return false;

            var cellA = grid[ax, ay];

            for (int i = 0; i < _candidateCount; i++)
            {
                var bCoord = _coords[i];

                if (bCoord == matchCoord) continue;

                if (!model.IsInRange(bCoord)) continue;

                if (!model.IsCellActive(bCoord)) continue;

                var bx = bCoord.x;
                var by = bCoord.y;

                var objB = model.GetGridObject(bx, by);
                
                if (!GridShuffleCalcUtil.IsSwapCandidate(objB)) continue;

                var cellB = grid[bx, by];

                if (cellA.TypeId == cellB.TypeId) continue;

                if (!GridMatchCalcUtil.IsCellsRegular(objA, objB)) continue;

                if (GridMatchCalcUtil.WouldSwapCreateMatch(model, grid, new Vector2Int(ax, ay), new Vector2Int(bx, by), objA.TypeId, objB.TypeId)) continue;

                model.Swap(matchCoord, bCoord);
                GridShuffleCalcUtil.SwapGridCells(grid, ax, ay, bx, by);
                return true;
            }

            return false;
        }

        private void ForceCreateAnyMove_NoTypeChange(IGridModel model, GridObjectType[,] grid)
        {
            for (int i = 0; i < _candidateCount; i++)
            {
                var a = _objs[i];
                if (!GridShuffleCalcUtil.IsSwapCandidate(a)) continue;

                var typeId = a.TypeId;

                BaseGridObject b = null;
                BaseGridObject c = null;

                for (int j = i + 1; j < _candidateCount; j++)
                {
                    var o = _objs[j];
                    if (!GridShuffleCalcUtil.IsSwapCandidate(o)) continue;
                    if (o.TypeId != typeId) continue;

                    if (!b)
                    {
                        b = o; 
                        continue;
                    }
                    
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

                        if (!GridShuffleCalcUtil.IsCellUsableForMove(model, p0) || !GridShuffleCalcUtil.IsCellUsableForMove(model, p1) || 
                            !GridShuffleCalcUtil.IsCellUsableForMove(model, p2) || !GridShuffleCalcUtil.IsCellUsableForMove(model, pGap))
                            continue;

                        var gapObj = model.GetGridObject(pGap);
                        
                        if (gapObj && gapObj.TypeId == typeId) continue;

                        GridShuffleCalcUtil.PlaceObjectAt(model, grid, a, p0);
                        GridShuffleCalcUtil.PlaceObjectAt(model, grid, b, p1);
                        GridShuffleCalcUtil.PlaceObjectAt(model, grid, c, p2);
                        return;
                    }
                }
            }
        }

        private void EnsureTaskCapacity(int count)
        {
            if (_animTasks.Length != count)
                Array.Resize(ref _animTasks, count);
        }
    }
}