using System;
using System.Collections.Generic;
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
            var forcedCenter = Context.MergeCenterCoord;
            Context.MergeCenterCoord = null;

            var model = Context.GridModel;
            var width = model.Width;
            var height = model.Height;

            var visited = new bool[width, height];
            var capacity = width * height;

            _animationTasks.Clear();

            if (_coordBuffer.Length < capacity)
                _coordBuffer = new Vector2Int[capacity];

            var grid = model.BuildGridTypeData();

            if (forcedCenter.HasValue)
            {
                var fc = forcedCenter.Value;
                ScanRect(fc.x, fc.y, fc.x + 1, fc.y + 1, 3, 999);
            }

            ScanRect(0, 0, width, height, 4, 999);
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

                _animationTasks.Add(ResolveGroupAsync(model, matchMask, group, typeId, forcedCenter));
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

        private UniTask ResolveGroupAsync(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int typeId, Vector2Int? forcedCenter)
        {
            if (GridMatchCalcUtil.IsAnyGroupObjectFall(model, group)) return UniTask.CompletedTask;

            var boosterType = GridBoosterDecisionUtil.DecideBoosterTypeFromGroup(model, matchMask, group, typeId);

            if (!boosterType.HasValue)
            {
                _destructionHandler.DestroyGroup(group);
                return UniTask.CompletedTask;
            }

            var isForcedCenterInGroup = forcedCenter.HasValue && group.Contains(forcedCenter.Value);
            var centerCoord = isForcedCenterInGroup ? forcedCenter.Value : GridBoosterDecisionUtil.SelectMergeCenter(group);

            var mergeObjs = GridMatchCalcUtil.GetMergedGroupObject(group, model);
            var type = boosterType.Value;

            _destructionHandler.ClearGroupForMerge(group);

            return _mergeHandler.PlayMergeAnimationAsync(mergeObjs, centerCoord)
                                .ContinueWith(() => OnCompleteAnimation(mergeObjs, centerCoord, type));
        }

        private void OnCompleteAnimation(BaseGridObject[] mergeObjs, Vector2Int centerCoord, BoosterType boosterType)
        {
            _destructionHandler.ReleaseObjects(mergeObjs);
            _mergeHandler.SpawnBooster(centerCoord, boosterType);
        }
    }
}