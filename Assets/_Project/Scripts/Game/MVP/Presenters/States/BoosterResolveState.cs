using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public class BoosterResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        protected override void OnEnter()
        {
            Context.RefillResolveRequested = false;
            ResolveBoosters();
        }

        private void ResolveBoosters()
        {
            var hasActions = Context.PendingBoosterActions is { Count: > 0 };

            if (hasActions)
            {
                ApplyPendingActions();
            }

            Context.ResolvedAnyBooster = hasActions;
            
            RequestExit();
        }
        
        private void ApplyPendingActions()
        {
            var model = Context.Model;
            var markData = new CellImpactMarkData[model.Width, model.Height];
            MarkPendingActions(model, markData);
            ApplyMarkedActions(model, markData);
        }

        private void MarkPendingActions(IGridModel model, CellImpactMarkData[,] markData)
        {
            var queue = new Queue<PendingBoosterAction>();
            var seen = new HashSet<BoosterActionKey>();
            
            for (int i = 0; i < Context.PendingBoosterActions.Count; i++)
            {
                EnqueueIfNew(Context.PendingBoosterActions[i]);
            }
            
            Context.PendingBoosterActions.Clear();

            while (queue.Count > 0)
            {
                var action = queue.Dequeue();
                
                var originObj = model.GetGridObject(action.OriginCoord);
                
                if (originObj)
                {
                    GridImpactMarker.MarkOriginObject(originObj, markData);
                }
                
                switch (action.BoosterAction)
                {
                    case RocketHorizontalAction hAction:
                        var horDirs = DirectionLookup.HorizontalDirections;
                        GridImpactMarker.MarkLinearArea(model, action, markData, hAction.DamageAmount, hAction.LineCount, horDirs, EnqueueIfNew);
                        break;
                    case RocketVerticalAction vAction:
                        var verDirs = DirectionLookup.VerticalDirections;
                        GridImpactMarker.MarkLinearArea(model, action, markData, vAction.DamageAmount, vAction.LineCount, verDirs, EnqueueIfNew);
                        break;
                    case BombAction bAction:
                        GridImpactMarker.MarkSquareArea(model, action, markData, bAction.DamageAmount, bAction.Radius, EnqueueIfNew);
                        break;
                    case FullGridRemoveAction fullRemoveAction:
                        GridImpactMarker.MarkAllAreaFromOrigin(model, action, markData, fullRemoveAction.DamageAmount, EnqueueIfNew);
                        break;
                }
            }
            
            return;
            
            void EnqueueIfNew(PendingBoosterAction boosterAction)
            {
                var key = new BoosterActionKey(boosterAction.OriginCoord, boosterAction.BoosterAction);
                if (!seen.Add(key)) return;
                queue.Enqueue(boosterAction);
            }
        }
        
        private void ApplyMarkedActions(IGridModel model, CellImpactMarkData[,] markData)
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
                        
                        if (obj is IBoosterActionSource source)
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
                            
                            if (obj is IBoosterActionSource source)
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