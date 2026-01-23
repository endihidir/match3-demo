using System;
using System.Collections.Generic;
using System.Linq;
using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class MatchResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        private UniTask[] _animationTasks = Array.Empty<UniTask>();

        private Vector2Int[] _coordBuffer = Array.Empty<Vector2Int>();
        
        private int _lastTaskCount;

        public MatchResolveState(GridStateContext context) : base(context) { }

        protected override void OnEnter()
        {
            Context.MatchResolveRequested = false;
            ResolveMatchesAsync().Forget();
        }

        private async UniTask ResolveMatchesAsync()
        {
            if (GridMatchMaskBuilder.TryBuildMatchMask(Context.Model, out var matchMask))
            {
                await ResolveMaskAsync(matchMask);
            }

            RequestExit();
        }

        private async UniTask ResolveMaskAsync(bool[,] matchMask)
        {
            var model = Context.Model;
            var width = model.Width;
            var height = model.Height;

            var visited = new bool[width, height];

            var capacity = width * height;

            if (_animationTasks.Length < capacity)
                _animationTasks = new UniTask[capacity];

            if (_coordBuffer.Length < capacity)
                _coordBuffer = new Vector2Int[capacity];

            var grid = model.BuildGridTypeData();

            var taskCount = 0;

            var hasForced = Context.HasMergeCenterCoordRequested;
            var forcedCoord = Context.MergeCenterCoord;

            for (int pass = 0; pass < (hasForced ? 2 : 1); pass++)
            {
                var onlyForced = hasForced && pass == 0;

                var startX = onlyForced ? forcedCoord.x : 0;
                var startY = onlyForced ? forcedCoord.y : 0;
                var endX = onlyForced ? forcedCoord.x + 1 : width;
                var endY = onlyForced ? forcedCoord.y + 1 : height;

                if (onlyForced)
                {
                    Scan(startX, startY, endX, endY, 3, 999);
                    continue;
                }

                Scan(startX, startY, endX, endY, 4, 999);
                Scan(startX, startY, endX, endY, 3, 3);
            }

            if (taskCount == 0) return;

            await WhenAllTasks(taskCount);
            
            return;

            void Scan(int startX, int startY, int endX, int endY, int minCount, int maxCount)
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

                var id = data.TypeId;
                if (id <= 0) return;

                var count = GridMatchCalcUtil.CollectMatchShapeFromCenter(model, grid, x, y, id, visited, _coordBuffer);
                if (count < minCount || count > maxCount) return;

                CommitVisited(count);

                var group = new List<Vector2Int>(count);

                for (int i = 0; i < count; i++)
                    group.Add(_coordBuffer[i]);

                _animationTasks[taskCount++] = ResolveGroupParallelAnimationAsync(model, matchMask, group, id);
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

        private UniTask WhenAllTasks(int taskCount)
        {
            if (taskCount == 0) return UniTask.CompletedTask;

            var end = _lastTaskCount;

            if (end > _animationTasks.Length) end = _animationTasks.Length;

            for (int i = taskCount; i < end; i++)
                _animationTasks[i] = UniTask.CompletedTask;

            _lastTaskCount = taskCount;

            return UniTask.WhenAll(_animationTasks);
        }

        private UniTask ResolveGroupParallelAnimationAsync(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            if (GridMatchCalcUtil.IsAnyGroupObjectFall(model, group)) return UniTask.CompletedTask;
            
            var boosterType = GridMatchBoosterDecision.DecideBoosterTypeFromGroup(model, matchMask, group, id);

            if (!boosterType.HasValue)
            {
                ReleaseGroup(model, group, true);
                return UniTask.CompletedTask;
            }

            var anyForced = TryConsumeForcedCenterCoord(group, out var forcedCoord);

            var centerCoord = anyForced ? forcedCoord : GridMatchBoosterDecision.SelectMergeCenter(group);

            var mergeObjs = GridMatchCalcUtil.GetMergedGroupObject(group, model, centerCoord);

            var boosterValue = boosterType.Value;

            GridMatchCalcUtil.SetNullMergedObjectCoords(model, group, centerCoord);
            
            var centerObj = model.GetGridObject(centerCoord);
            
            model.SetGridObject(centerCoord, null);
            
            return PlayMergeAnimationAsync(mergeObjs, centerCoord).ContinueWith(() => OnMergeComplete(mergeObjs, centerObj, centerCoord, boosterValue));
        }

        private void OnMergeComplete(BaseGridObject[] mergeObjs, BaseGridObject centerObj, Vector2Int centerCoord, BoosterType boosterValue)
        {
            ReleaseMergedObjects(mergeObjs);

            if (centerObj)
            {
                Context.Factory.ReleaseItem(centerObj);
            }
            
            SpawnBooster(centerCoord, boosterValue);
        }

        private bool TryConsumeForcedCenterCoord(List<Vector2Int> group, out Vector2Int forcedCoord)
        {
            if (!Context.HasMergeCenterCoordRequested)
            {
                forcedCoord = default;
                return false;
            }

            var coord = Context.MergeCenterCoord;

            if (group.Any(t => t == coord))
            {
                Context.HasMergeCenterCoordRequested = false;
                forcedCoord = coord;
                return true;
            }

            forcedCoord = default;
            return false;
        }

        private void ReleaseMergedObjects(BaseGridObject[] mergeObjs)
        {
            foreach (var obj in mergeObjs)
            {
                if (!obj) continue;

                Context.Factory.ReleaseItem(obj);
            }
        }

        private void ReleaseGroup(IGridModel model, List<Vector2Int> group, bool hasDamage = false, Vector2Int? exceptCoord = null)
        {
            foreach (var coord in group)
            {
                if (exceptCoord.HasValue && coord == exceptCoord.Value) continue;

                var obj = model.GetGridObject(coord);

                if (!obj) continue;

                if (hasDamage) ApplyNeighbourDamage(model, coord);

                Context.ReleaseAndSetNull(obj, coord);
            }
        }

        private void ApplyNeighbourDamage(IGridModel model, Vector2Int origin)
        {
            foreach (var linearDirection in DirectionLookup.LinearDirections)
            {
                if (!model.TryGetNeighbourCoord(origin, linearDirection, out var neighbourCoord)) continue;

                var obj = model.GetGridObject(neighbourCoord);

                if (!obj) continue;

                if (obj is not IDamageableItem damageable) continue;

                var result = damageable.TakeDamage(1, DamageSource.Match);

                if (result == DamageResult.Destroyed)
                {
                    Context.ReleaseAndSetNull(obj, neighbourCoord);
                }
            }
        }

        private async UniTask PlayMergeAnimationAsync(BaseGridObject[] mergeObjs, Vector2Int centerCoord)
        {
            var targetWorld = Context.View.GridToWorld(centerCoord);

            var tasks = new UniTask[mergeObjs.Length];

            for (int i = 0; i < mergeObjs.Length; i++)
            {
                var obj = mergeObjs[i];

                if (!obj)
                {
                    tasks[i] = UniTask.CompletedTask;
                    continue;
                }

                tasks[i] = obj.ItemAnimation.MoveTo(targetWorld).ToUniTask();
            }

            await UniTask.WhenAll(tasks);
        }

        private void SpawnBooster(Vector2Int pos, BoosterType boosterType)
        {
            var booster = Context.Factory.GetBoosterItem(boosterType);
            Context.Model.SetGridObject(pos, booster);
            booster.SetPosition(Context.View.GridToWorld(pos));
            booster.SetSpriteSize(Context.View.GetCellSize());
            booster.SetParent(Context.View.GridObjectsParent);
        }
    }
}