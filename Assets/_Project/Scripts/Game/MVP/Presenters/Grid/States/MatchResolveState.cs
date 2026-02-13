using System;
using System.Collections.Generic;
using System.Linq;
using Core.Extensions;
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

            if (_animationTasks.Length < capacity)
                _animationTasks = new UniTask[capacity];

            if (_coordBuffer.Length < capacity)
                _coordBuffer = new Vector2Int[capacity];

            var grid = model.BuildGridTypeData();

            var taskCount = 0;

            var hasForcedCenter = Context.HasMergeCenterCoordRequested;
            var forcedCenter = Context.MergeCenterCoord;

            if (hasForcedCenter)
            {
                // 1) Forced center pass
                ScanRect(forcedCenter.x, forcedCenter.y, forcedCenter.x + 1, forcedCenter.y + 1, 3, 999);
            }

            // 2) Booster (4+ or T/L/5 vs) pass
            ScanRect(0, 0, width, height, 4, 999);

            // 3) Regular 3 pass
            ScanRect(0, 0, width, height, 3, 3);

            if (taskCount == 0) return;

            await WhenAllTasks(taskCount);

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

                _animationTasks[taskCount++] = ResolveGroupParallelAnimationAsync(model, matchMask, group, typeId);
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

        private UniTask ResolveGroupParallelAnimationAsync(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int typeID)
        {
            var isAnyGroupObjectFall = GridMatchCalcUtil.IsAnyGroupObjectFall(model, group);

            if (isAnyGroupObjectFall) return UniTask.CompletedTask;
            
            var boosterType = GridBoosterDecisionUtil.DecideBoosterTypeFromGroup(model, matchMask, group, typeID);

            if (!boosterType.HasValue)
            {
                ReleaseGroup(model, group);
                return UniTask.CompletedTask;
            }

            var anyForced = TryConsumeForcedCenterCoord(group, out var forcedCoord);

            var centerCoord = anyForced ? forcedCoord : GridBoosterDecisionUtil.SelectMergeCenter(group);

            var mergeObjs = GridMatchCalcUtil.GetMergedGroupObject(group, model, centerCoord);

            var boosterValue = boosterType.Value;

            var centerObj = model.GetGridObject(centerCoord);
            
            SetNullMergedObjectCoords(model, group);
            
            return PlayMergeAnimationAsync(mergeObjs, centerCoord).ContinueWith(() => OnMergeComplete(mergeObjs, centerObj, centerCoord, boosterValue));
        }
        
        private void SetNullMergedObjectCoords(IGridModel model, List<Vector2Int> group)
        {
            foreach (var coord in group)
            {
                ApplyNeighbourDamage(model, coord);
                
                model.SetGridObject(coord, null);
            }
        }

        private void OnMergeComplete(BaseGridObject[] mergeObjs, BaseGridObject centerObj, Vector2Int centerCoord, BoosterType boosterValue)
        {
            ReleaseMergedObjects(mergeObjs);

            if (centerObj)
            {
                Context.GridItemFactory.ReleaseItem(centerObj);
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

                Context.GridItemFactory.ReleaseItem(obj);
            }
        }

        private void ReleaseGroup(IGridModel model, List<Vector2Int> group)
        {
            foreach (var coord in group)
            {
                var obj = model.GetGridObject(coord);

                if (!obj) continue;

                ApplyNeighbourDamage(model, coord);
                
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

                if (obj is not IDamageableGridObject damageableItem) continue;

                var result = damageableItem.TakeDamage(1, GridDamageSource.Match);

                if (result == GridDamageResult.Destroyed)
                {
                    Context.ProgressGoal(damageableItem, obj.Coord, obj.SpriteRenderer.size);
                    
                    Context.ReleaseAndSetNull(obj, obj.Coord);
                }
            }
        }

        private async UniTask PlayMergeAnimationAsync(BaseGridObject[] mergeObjs, Vector2Int centerCoord)
        {
            var targetWorld = Context.GridView.GridToWorld(centerCoord);

            var tasks = new UniTask[mergeObjs.Length];

            for (int i = 0; i < mergeObjs.Length; i++)
            {
                var obj = mergeObjs[i];

                if (!obj)
                {
                    tasks[i] = UniTask.CompletedTask;
                    continue;
                }

                var tween = obj.Animation.MoveTo(targetWorld);

                tasks[i] = tween?.ToUniTask() ?? UniTask.CompletedTask;
            }

            await UniTask.WhenAll(tasks);
        }

        private void SpawnBooster(Vector2Int pos, BoosterType boosterType)
        {
            var booster = Context.GridItemFactory.GetBoosterItem(boosterType);
            Context.GridModel.SetGridObject(pos, booster);
            booster.SetPosition(Context.GridView.GridToWorld(pos));
            booster.SetSpriteSize(Context.GridView.GetCellSize());
            booster.SetParent(Context.GridView.GridObjectsParent);
        }
    }
}