using System.Collections.Generic;
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
                    if (group == null || group.Count == 0) continue;
                    
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

            var centerCoord = GridBoosterDecision.SelectMergeCenter(group, Context.HasForcedBoosterSpawnCoord, Context.ForcedBoosterSpawnCoord);

            Context.HasForcedBoosterSpawnCoord = false;

            await PlayMergeAnimationAsync(group, centerCoord);

            ReleaseGroup(model, group, false, centerCoord);

            var centerObj = model.GetGridObject(centerCoord);
            
            if (centerObj)
            {
                Context.Factory.ReleaseItem(centerObj);
                model.SetGridObject(centerCoord, null);
            }

            SpawnBooster(centerCoord, boosterType.Value);
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
        
        private async UniTask PlayMergeAnimationAsync(List<Vector2Int> group, Vector2Int spawnCoord)
        {
            var tasks = new List<UniTask>(group.Count);
            var targetWorld = Context.View.GridToWorld(spawnCoord);
            
            for (int i = 0; i < group.Count; i++)
            {
                var coord = group[i];
                if (coord == spawnCoord) continue;
                var obj = Context.Model.GetGridObject(coord);
                if (!obj) continue;
                var tween = obj.ItemAnimation.Move(targetWorld);
                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
            }

            if (tasks.Count > 0) 
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