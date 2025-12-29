using System;
using System.Collections.Generic;
using System.Linq;
using Core.Config;
using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class ResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitTime => true;

        private const float MergeMoveDuration = 0.1f;
        
        private const Ease MergeEase = Ease.InOutQuad;
        
        protected override void OnEnter()
        {
            Context.RefillResolveRequested = false;
            
            ResolveAsync().Forget();
        }

        private async UniTask ResolveAsync()
        {
            // TODO: RESOLVE PENDING EFFECTS FIRST IF EXIST AND RETURN!!! IF THERE IS NOT ANY PENDING EFFECTS THEN RESOLVE MATCHES!!!
            
            if (Context.PendingEffects is { Count: > 0 })
            {
                ApplyPendingEffects();
                
                Context.ResolvedAnyMatch = true;
                
                RequestExit();
                
                return;
            }
            
            var matchMask = GridMatchMaskBuilder.BuildMatchMask(Context.Model, out var hasMatch);

            if (hasMatch)
            {
                await ResolveFromMask(matchMask);
            }
            
            Context.ResolvedAnyMatch = hasMatch;
            
            RequestExit();
        }

        private void ApplyPendingEffects()
        {
            var model = Context.Model;
            var markData = new CellEffectMark[model.Width, model.Height];
            MarkPendingEffects(model, markData);
            ApplyMarkedEffects(model, markData);
        }

        private void MarkPendingEffects(IGridModel model, CellEffectMark[,] markData)
        {
            var queue = new Queue<PendingEffect>();
            var seen = new HashSet<EffectKey>();
            
            for (int i = 0; i < Context.PendingEffects.Count; i++)
            {
                EnqueueIfNew(Context.PendingEffects[i]);
            }
            
            Context.PendingEffects.Clear();

            while (queue.Count > 0)
            {
                var effect = queue.Dequeue();
                
                var originObj = model.GetGridObject(effect.OriginCoord);
                
                if (originObj)
                {
                    GridMarkRules.MarkOriginObject(originObj, markData);
                }
                
                switch (effect.BoosterAction)
                {
                    case RocketHorizontalAction hAction:
                        var horDirs = DirectionLookup.HorizontalDirections;
                        GridMarkRules.MarkLinearArea(model, effect, markData, hAction.DamageAmount, hAction.LineCount, horDirs, EnqueueIfNew);
                        break;
                    case RocketVerticalAction vAction:
                        var verDirs = DirectionLookup.VerticalDirections;
                        GridMarkRules.MarkLinearArea(model, effect, markData, vAction.DamageAmount, vAction.LineCount, verDirs, EnqueueIfNew);
                        break;
                    case BombAction bAction:
                        GridMarkRules.MarkSquareArea(model, effect, markData, bAction.DamageAmount, bAction.Radius, EnqueueIfNew);
                        break;
                    case FullGridRemoveAction fullRemoveAction:
                        GridMarkRules.MarkAllAreaFromOrigin(model, effect, markData, fullRemoveAction.DamageAmount, EnqueueIfNew);
                        break;
                }
            }
            
            return;
            
            void EnqueueIfNew(PendingEffect effect)
            {
                var key = new EffectKey(effect.OriginCoord, effect.BoosterAction);
                if (!seen.Add(key)) return;
                queue.Enqueue(effect);
            }
        }
        
        private void ApplyMarkedEffects(IGridModel model, CellEffectMark[,] markData)
        { 
            for (int x = 0; x < model.Width; x++)
            {
                for (int y = 0; y < model.Height; y++)
                {
                    var coord = new Vector2Int(x, y);
                    var obj =  model.GetGridObject(coord);
                    if(!obj) continue;
                    
                    var data = markData[x, y];

                    if (data.Remove)
                    {
                        ClearAndRelease(coord, obj);
                        
                        if (obj is ITriggerEffectSource source)
                        {
                            // TODO: play booster effect!
                        }

                        // TODO: play remove effect!
                        continue;
                    }

                    if (data.HasDamage && obj is IDamageableItem damageableItem)
                    {
                        var damageResult = damageableItem.TakeDamage(data.DamageAmount, data.DamageSource);

                        if (damageResult == DamageResult.Damaged)
                        {
                            // TODO: play damaged effect!
                        }
                        else if (damageResult == DamageResult.Destroyed)
                        {
                            ClearAndRelease(coord, obj);
                            
                            if (obj is ITriggerEffectSource source)
                            {
                                // TODO: play booster effect!
                            }
                            
                            // TODO: play remove effect!
                        }
                        continue;
                    }
                }
            }
            return;
            
            void ClearAndRelease(Vector2Int coord, BaseGridObject obj)
            {
                model.SetGridObject(coord, null);
                Context.Factory.ReleaseItem(obj);
            }
        }

        private async UniTask ResolveFromMask(bool[,] matchMask)
        {
            var model = Context.Model;
            var width = model.Width;
            var height = model.Height;

            var grid = model.BuildTypeDataGrid();
            var visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!matchMask[x, y]) continue;
                    if (visited[x, y]) continue;

                    var id = grid[x, y].TypeId;
                    if (id <= 0) continue;

                    var coord = new Vector2Int(x, y);
                    var group = GridMatchGroupCollector.CollectGroupFromMask(matchMask, visited, model, grid, coord, id);
                    if (group == null || group.Count == 0) continue;

                    await ResolveGroup(model, grid, matchMask, group, id);
                }
            }
        }

        private async UniTask ResolveGroup(IGridModel model, GridObjectType[,] grid, bool[,] matchMask, List<Vector2Int> group, int id)
        {
            var boosterType = GridBoosterDecision.DecideBoosterTypeFromGroup(model, grid, matchMask, group, id);

            if (!boosterType.HasValue)
            {
                ReleaseGroup(model, group, true);
                return;
            }

            var centerCoord = GridBoosterDecision.SelectMergeCenter(group, Context.HasForcedBoosterSpawnCoord, Context.ForcedBoosterSpawnCoord);

            Context.HasForcedBoosterSpawnCoord = false;

            await PlayMergeAnimation(group, centerCoord);

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
        
        private async UniTask PlayMergeAnimation(List<Vector2Int> group, Vector2Int spawn)
        {
            var tasks = new List<UniTask>(group.Count);
            var targetWorld = Context.View.GridToWorld(spawn);
            
            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];
                if (c == spawn) continue;
                var obj = Context.Model.GetGridObject(c);
                if (!obj) continue;
                var tween = obj.ItemAnimation.Move(targetWorld, MergeMoveDuration, MergeEase);
                tasks.Add(tween.AsyncWaitForCompletion().AsUniTask());
            }

            if (tasks.Count > 0) await UniTask.WhenAll(tasks);
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