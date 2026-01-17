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
            
            var taskCount = 0;
            
            if (Context.HasForcedBoosterSpawnCoord)
            {
                var forcedCoord = Context.ForcedBoosterSpawnCoord;

                if (model.IsInRange(forcedCoord) && matchMask[forcedCoord.x, forcedCoord.y] && !visited[forcedCoord.x, forcedCoord.y])
                {
                    var forcedObj = model.GetGridObject(forcedCoord);

                    if (forcedObj)
                    {
                        var forcedGroup = GridMatchGroupCollector.CollectGroupFromMask(model, matchMask, visited, forcedCoord);

                        if (forcedGroup is { Count: > 2 })
                        {
                            var forcedId = forcedObj.TypeId;

                            if (forcedId > 0)
                            {
                                _animationTasks[taskCount++] = ResolveGroupParallelAnimationAsync(model, matchMask, forcedGroup, forcedId);
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!matchMask[x, y]) continue;
                    if (visited[x, y]) continue;

                    var obj = model.GetGridObject(x, y);
                    if (!obj) continue;

                    var coord = new Vector2Int(x, y);
                    var group = GridMatchGroupCollector.CollectGroupFromMask(model, matchMask, visited, coord);
                    if (group == null || group.Count < 3) continue;

                    var id = obj.TypeId;
                    if (id <= 0) continue;
                    
                    _animationTasks[taskCount++] = ResolveGroupParallelAnimationAsync(model, matchMask, group, id);
                }
            }

            if (taskCount == 0) return;
            
            await WhenAllTasks(taskCount);
        }

        private UniTask WhenAllTasks(int taskCount)
        {
            if (taskCount == 0)
            {
                _lastTaskCount = 0;
                return UniTask.CompletedTask;
            }

            var end = _lastTaskCount;
            
            if (end > _animationTasks.Length) end = _animationTasks.Length;

            for (int i = taskCount; i < end; i++)
                _animationTasks[i] = UniTask.CompletedTask;

            _lastTaskCount = taskCount;

            return UniTask.WhenAll(_animationTasks);
        }
        
        private UniTask ResolveGroupParallelAnimationAsync(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            var boosterType = GridBoosterDecision.DecideBoosterTypeFromGroup(model, matchMask, group, id);

            if (!boosterType.HasValue)
            {
                ReleaseGroup(model, group, true);
                return UniTask.CompletedTask;
            }

            var anyForced = TryConsumeForcedCenterCoord(group, out var forcedCoord);
            var centerCoord = anyForced ? forcedCoord : GridBoosterDecision.SelectMergeCenter(group);

            var mergeObjs = GetMergedGroupObject(group, model, centerCoord);
            
            SetNullMergedObjectCoords(model, group, centerCoord);
            
            var centerObj = model.GetGridObject(centerCoord);

            if (centerObj)
            {
                Context.ReleaseAndSetNull(centerObj, centerCoord);
            }

            var boosterValue = boosterType.Value;
            
            return PlayMergeAnimationAsync(mergeObjs, centerCoord).ContinueWith(() =>
            {
                ReleaseMergedObjects(mergeObjs);
                SpawnBooster(centerCoord, boosterValue);
            });
        }

        private bool TryConsumeForcedCenterCoord(List<Vector2Int> group, out Vector2Int forcedCoord)
        {
            if (!Context.HasForcedBoosterSpawnCoord)
            {
                forcedCoord = default;
                return false;
            }

            var forced = Context.ForcedBoosterSpawnCoord;

            if (group.Any(t => t == forced))
            {
                Context.HasForcedBoosterSpawnCoord = false;
                forcedCoord = forced;
                return true;
            }

            forcedCoord = default;
            return false;
        }

        private static BaseGridObject[] GetMergedGroupObject(List<Vector2Int> group, IGridModel model, Vector2Int centerCoord)
        {
            var mergeObjs = new BaseGridObject[group.Count - 1];
            var index = 0;

            foreach (var coord in group)
            {
                if (coord == centerCoord) continue;
                var obj = model.GetGridObject(coord);
                if (!obj) continue;
                mergeObjs[index++] = obj;
            }

            return mergeObjs;
        }

        private static void SetNullMergedObjectCoords(IGridModel model, List<Vector2Int> group, Vector2Int centerCoord)
        {
            foreach (var coord in group.Where(coord => coord != centerCoord))
            {
                model.SetGridObject(coord, null);
            }
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

                tasks[i] = obj.ItemAnimation.Move(targetWorld, .6f).ToUniTask();
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
