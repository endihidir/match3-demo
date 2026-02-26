using System;
using System.Collections.Generic;
using System.Linq;
using Game.Grid.Contexts;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using Game.Grid.Item;
using Game.Models;
using UnityEngine;

namespace Game.Grid.States
{
    public sealed class MatchResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        private readonly IMatchDestructionHandler _destructionHandler;
        private readonly IMatchMergeHandler _mergeHandler;
        private readonly List<UniTask> _animationTasks = new(256);
        
        private Vector2Int[] _coordBuffer = Array.Empty<Vector2Int>();

        public MatchResolveState(GridStateContext context, IMatchDestructionHandler destructionHandler, IMatchMergeHandler mergeHandler) : base(context)
        {
            _destructionHandler = destructionHandler;
            _mergeHandler = mergeHandler;
        }

        protected override void OnEnter()
        {
            Context.MatchResolveRequested = false;
            ResolveMatchesAsync().Forget();
        }

        private async UniTask ResolveMatchesAsync()
        {
            if (GridMatchMaskBuilder.TryBuildMatchMask(Context.GridModel, out var matchMask))
            {
                await ResolveMaskAsync(matchMask);
            }

            Context.RaiseDestructionStateComplete();
            RequestExit();
        }

        private async UniTask ResolveMaskAsync(bool[,] matchMask)
        {
            var model = Context.GridModel;
            var width = model.Width;
            var height = model.Height;

            var visited = new bool[width, height];

            var capacity = width * height;
            
            _animationTasks.Clear();

            if (_coordBuffer.Length < capacity)
                _coordBuffer = new Vector2Int[capacity];

            var grid = model.BuildGridTypeData();

            // 1) Forced center pass (priority)
            if (Context.MergeCenterCoord.HasValue)
            {
                var forcedCenter = Context.MergeCenterCoord.Value;
                // 1) Forced center pass
                ScanRect(forcedCenter.x, forcedCenter.y, forcedCenter.x + 1, forcedCenter.y + 1, 3, 999);
            }

            // 2) Booster (4+ or T/L/5 vs) pass
            ScanRect(0, 0, width, height, 4, 999);

            // 3) Regular 3 pass
            ScanRect(0, 0, width, height, 3, 3);

            await UniTask.WhenAll(_animationTasks);

            return;

            void ScanRect(int startX, int startY, int endX, int endY, int minCount, int maxCount)
            {
                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                        TryScheduleAt(x, y, minCount, maxCount);
                }
            }

            void TryScheduleAt(int x, int y, int minCount, int maxCount)
            {
                if (!matchMask[x, y]) return;
                if (visited[x, y]) return;

                var data = grid[x, y];
                if (!GridMatchCalcUtil.IsRegularItem(data)) return;

                var typeId = data.TypeId;
                if (typeId <= 0) return;

                var count = GridMatchCalcUtil.CollectMatchShapeFromCenter(model, grid, x, y, typeId, visited, _coordBuffer);
                if (count < minCount || count > maxCount) return;

                CommitVisited(count);

                var group = new List<Vector2Int>(count);
                for (int i = 0; i < count; i++)
                    group.Add(_coordBuffer[i]);

                var task = ResolveGroupAsync(model, matchMask, group, typeId);
                
                _animationTasks.Add(task);
            }

            void CommitVisited(int count)
            {
                for (int i = 0; i < count; i++)
                {
                    var c = _coordBuffer[i];
                    visited[c.x, c.y] = true;
                }
            }
        }

        private UniTask ResolveGroupAsync(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int typeId)
        {
            if (GridMatchCalcUtil.IsAnyGroupObjectFall(model, group)) return UniTask.CompletedTask;

            var boosterType = GridBoosterDecisionUtil.DecideBoosterTypeFromGroup(model, matchMask, group, typeId);

            if (!boosterType.HasValue)
            {
                _destructionHandler.DestroyGroup(group);
                return UniTask.CompletedTask;
            }

            var anyForced = TryConsumeForcedCenterCoord(group, out var forcedCoord);
            var centerCoord = anyForced ? forcedCoord : GridBoosterDecisionUtil.SelectMergeCenter(group);

            var mergeObjs = GridMatchCalcUtil.GetMergedGroupObject(group, model, centerCoord);
            var type = boosterType.Value;
            var centerObj = model.GetGridObject(centerCoord);
            
            _destructionHandler.ClearGroupForMerge(group);

            return _mergeHandler.PlayMergeAnimationAsync(mergeObjs, centerCoord)
                                .ContinueWith(() => OnCompleteAnimation(mergeObjs, centerObj, centerCoord, type));
        }

        private void OnCompleteAnimation(BaseGridObject[] mergeObjs, BaseGridObject centerObj, Vector2Int centerCoord, BoosterType boosterType)
        {
            _destructionHandler.ReleaseObjects(mergeObjs);
            _destructionHandler.ReleaseObject(centerObj);
            _mergeHandler.SpawnBooster(centerCoord, boosterType);
        }

        private bool TryConsumeForcedCenterCoord(List<Vector2Int> group, out Vector2Int forcedCoord)
        {
            if (!Context.MergeCenterCoord.HasValue)
            {
                forcedCoord = default;
                return false;
            }

            var coord = Context.MergeCenterCoord.Value;

            if (group.Any(t => t == coord))
            {
                Context.MergeCenterCoord = null;
                forcedCoord = coord;
                return true;
            }

            forcedCoord = default;
            return false;
        }
    }
}