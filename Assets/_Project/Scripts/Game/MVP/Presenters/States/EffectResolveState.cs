using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public class EffectResolveState : StateBase<GridStateContext>
    {
        protected override void OnEnter()
        {
            Context.RefillResolveRequested = false;
            ResolveEffects();
        }

        private void ResolveEffects()
        {
            var hasEffects = Context.PendingEffects is { Count: > 0 };

            if (hasEffects)
            {
                ApplyPendingEffects();
            }

            Context.ResolvedAnyEffect = hasEffects;
            
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

                        // TODO: play destroy effect!
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
                            
                            // TODO: play destroy effect!
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
    }
}