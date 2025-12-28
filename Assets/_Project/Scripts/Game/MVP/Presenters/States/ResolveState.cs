using System;
using System.Collections.Generic;
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
    public struct CellEffectMark
    {
        public bool Remove { get; private set; }
        public int DamageAmount { get; private set; }
        public DamageSource DamageSource { get; private set; }
        
        public void MarkRemove() => Remove = true;
        public void UnMarkRemove() => Remove = false;
        public void MarkDamage(int damageAmount, DamageSource damageSource)
        {
            DamageAmount = damageAmount;
            DamageSource |= damageSource;
        }
        public void ClearDamage()
        {
            DamageSource = 0;
            DamageSource = DamageSource.None;
        }
    }
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
                Context.PendingEffects.Clear();
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
            var width = Context.Model.Width;
            var height = Context.Model.Height;
            var markData = new CellEffectMark[width, height];
            
            foreach (var effect in Context.PendingEffects)
            {
                switch (effect.BoosterAction)
                {
                    case RocketHorizontalAction hAction:
                        MarkLinear(effect, markData, hAction.DamageAmount, hAction.LineCount, DirectionLookup.HorizontalDirections);
                        break;
                    case RocketVerticalAction vAction:
                        MarkLinear(effect, markData, vAction.DamageAmount, vAction.LineCount, DirectionLookup.VerticalDirections);
                        break;
                    case BombAction bAction:
                        break;
                    case FullGridRemoveAction fullRemoveAction:
                        break;
                }
            }
        }

        private void MarkLinear(PendingEffect effect, CellEffectMark[,] markData, int damageAmount, int lineCount, Vector2Int[] directions)
        {
            foreach (var leftRight in directions)
            {
                VisitLineExceptSelf(effect.OriginCoord, leftRight, lineCount, obj =>
                {
                    var coord = obj.Coord;
                    
                    ref var cell = ref markData[coord.x, coord.y];
                                
                    if (obj is IDamageableItem damageableItem)
                    {
                        cell.MarkDamage(damageAmount, DamageSource.Booster);
                    }
                    else if (obj is ITriggerEffectSource triggerEffectSource)
                    {
                        if (triggerEffectSource.TryBuildEffect(coord, out var pendingEffect))
                        {
                            Context.PendingEffects.Add(pendingEffect);
                        }
                    }
                    else
                    {
                        cell.MarkRemove();
                    }
                });
            }
        }

        private void VisitLineExceptSelf(Vector2Int origin, Vector2Int dir, int lineCount, Action<BaseGridObject> visit)
        {
            if (lineCount <= 0) return;

            var model = Context.Model;
            var width = model.Width;
            var height = model.Height;

            var half = (lineCount - 1) / 2;
            var start = (lineCount & 1) == 1 ? -half : 0;
            var end = (lineCount & 1) == 1 ? half : lineCount - 1;

            if (dir.x != 0)
            {
                for (int dy = start; dy <= end; dy++)
                {
                    var y = origin.y + dy;
                    if (y < 0 || y >= height) continue;

                    for (int x = 0; x < width; x++)
                    {
                        var c = new Vector2Int(x, y);
                        if (c == origin) continue;

                        var obj = model.GetGridObject(c);
                        if (obj) visit(obj);
                    }
                }

                return;
            }

            for (int dx = start; dx <= end; dx++)
            {
                var x = origin.x + dx;
                if (x < 0 || x >= width) continue;

                for (int y = 0; y < height; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (c == origin) continue;

                    var obj = model.GetGridObject(c);
                    if (obj) visit(obj);
                }
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
                ReleaseGroup(model, group);
                return;
            }

            var centerCoord = GridBoosterDecision.SelectMergeCenter(group, Context.HasForcedBoosterSpawnCoord, Context.ForcedBoosterSpawnCoord);

            Context.HasForcedBoosterSpawnCoord = false;

            await PlayMergeAnimation(group, centerCoord);

            ReleaseGroup(model, group, centerCoord);

            var centerObj = model.GetGridObject(centerCoord);
            
            if (centerObj)
            {
                Context.Factory.ReleaseItem(centerObj);
                model.SetGridObject(centerCoord, null);
            }

            SpawnBooster(centerCoord, boosterType.Value);
        }
        
        private void ReleaseGroup(IGridModel model, List<Vector2Int> group, Vector2Int? exceptCoord = null)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];
                if (exceptCoord.HasValue && c == exceptCoord.Value) continue;

                var obj = model.GetGridObject(c);
                if (!obj) continue;

                Context.Factory.ReleaseItem(obj);
                model.SetGridObject(c, null);
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