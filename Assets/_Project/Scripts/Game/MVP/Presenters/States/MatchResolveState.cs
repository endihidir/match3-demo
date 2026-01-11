using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class MatchResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

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
                                await ResolveGroupAsync(model, matchMask, forcedGroup, forcedId);
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

                    await ResolveGroupAsync(model, matchMask, group, id);
                }
            }
        }

        private async UniTask ResolveGroupAsync(IGridModel model, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            var boosterType = GridBoosterDecision.DecideBoosterTypeFromGroup(model, matchMask, group, id);
            
            if (!boosterType.HasValue)
            {
                ReleaseGroup(model, group, true);
                return;
            }

            var anyForced = TryConsumeForcedCenterCoord(group, out var forcedCoord);
            
            var centerCoord = anyForced ? forcedCoord : GridBoosterDecision.SelectMergeCenter(group);
            
            var mergeObjs = GridMatchResolveUtil.GetMergedGroupObject(group, model, centerCoord);
            
            GridMatchResolveUtil.SetNullMergedObjectCoords(model, group, centerCoord);
            
            await PlayMergeAnimationAsync(mergeObjs, centerCoord);
            
            ReleaseMergedObjects(mergeObjs);

            var centerObj = model.GetGridObject(centerCoord);

            if (centerObj)
            {
                Context.Factory.ReleaseItem(centerObj);
                model.SetGridObject(centerCoord, null);
            }

            SpawnBooster(centerCoord, boosterType.Value);
        }
        
        private bool TryConsumeForcedCenterCoord(List<Vector2Int> group, out Vector2Int forcedCoord)
        {
            if (!Context.HasForcedBoosterSpawnCoord)
            {
                forcedCoord = default;
                return false;
            }

            var forced = Context.ForcedBoosterSpawnCoord;

            for (int i = 0; i < group.Count; i++)
            {
                if (group[i] != forced) continue;

                Context.HasForcedBoosterSpawnCoord = false;
                forcedCoord = forced;
                return true;
            }

            forcedCoord = default;
            return false;
        }

        private void ReleaseMergedObjects(BaseGridObject[] mergeObjs)
        {
            for (int i = 0; i < mergeObjs.Length; i++)
            {
                var obj = mergeObjs[i];
                if (!obj) continue;
                Context.Factory.ReleaseItem(obj);
            }
        }

        private void ReleaseGroup(IGridModel model, List<Vector2Int> group, bool hasDamage = false, Vector2Int? exceptCoord = null)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var coord = group[i];
                if (exceptCoord.HasValue && coord == exceptCoord.Value) continue;

                var obj = model.GetGridObject(coord);
                if (!obj) continue;

                if (hasDamage) ApplyNeighbourDamage(model, coord);

                Context.Factory.ReleaseItem(obj);
                model.SetGridObject(coord, null);
            }
        }

        private void ApplyNeighbourDamage(IGridModel model, Vector2Int origin)
        {
            foreach (var linearDirection in DirectionLookup.LinearDirections)
            {
                var neighbour = origin + linearDirection;
                if (!model.IsInRange(neighbour)) continue;

                var obj = model.GetGridObject(neighbour);
                if (!obj) continue;

                if (obj is IDamageableItem damageable)
                {
                    var result = damageable.TakeDamage(1, DamageSource.Match);

                    if (result == DamageResult.Destroyed)
                    {
                        Context.Factory.ReleaseItem(obj);
                        model.SetGridObject(neighbour, null);
                    }
                }
            }
        }
        
        private async Task PlayMergeAnimationAsync(BaseGridObject[] mergeObjs, Vector2Int centerCoord)
        {
            var targetWorld = Context.View.GridToWorld(centerCoord);
            var tasks = new Task[mergeObjs.Length];

            for (int i = 0; i < mergeObjs.Length; i++)
            {
                var obj = mergeObjs[i];
                if (!obj) continue;
                var tween = obj.ItemAnimation.Move(targetWorld);
                tasks[i] = tween.AsyncWaitForCompletion();
            }

            await Task.WhenAll(tasks);
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