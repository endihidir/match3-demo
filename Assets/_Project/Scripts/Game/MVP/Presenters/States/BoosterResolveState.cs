using System.Collections.Generic;
using Core.Config;
using Core.Item;
using Core.Models;
using Core.StateMachineCore;
using Core.Utils;
using Core.Views;
using UnityEngine;

namespace Core.Handlers
{
    public class BoosterResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        public BoosterResolveState(GridStateContext context) : base(context) { }

        protected override void OnEnter()
        {
            ApplyPendingActions();
            RequestExit();
        }
        
        private void ApplyPendingActions()
        {
            var model = Context.Model;
            var view = Context.View;
            MarkPendingActions(model, out var markData);
            ApplyMarkedActions(model, view, markData);
        }

        private void MarkPendingActions(IGridModel model, out CellImpactMarkData[,] markData)
        {
            markData = new CellImpactMarkData[model.Width, model.Height]; 
            var queue = new Queue<BoosterActionContext>();
            var seen = new HashSet<BoosterActionKey>();
            
            for (int i = 0; i < Context.PendingBoosterActions.Count; i++)
            {
                EnqueueIfNew(Context.PendingBoosterActions[i]);
            }
            
            Context.PendingBoosterActions.Clear();

            while (queue.Count > 0)
            {
                var action = queue.Dequeue();
                
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
                }
            }

            if (!Context.HasInputTriggeredBooster) return;
            
            Context.HasInputTriggeredBooster = false;
                
            var targetCoord = Context.InputTriggeredBoosterCoord;

            if (!GridMatchMaskBuilder.TryBuildMatchMask(model, out var matchMask)) return;
                
            if (!GridMatchCalcUtil.TryBuildBoosterGroupMaskAt(model, targetCoord, matchMask, out var boosterMask)) return;
                    
            for (int x = 0; x < model.Width; x++)
            {
                for (int y = 0; y < model.Height; y++)
                {
                    if (!boosterMask[x, y]) continue;
                                
                    markData[x, y].UnMarkRemove();
                }
            }

            return;
            
            void EnqueueIfNew(BoosterActionContext boosterActionContext)
            {
                var key = new BoosterActionKey(boosterActionContext.OriginCoord, boosterActionContext.BoosterAction);
                
                if (!seen.Add(key)) return;
                
                // It is using for triggered boosters!
                if (boosterActionContext.GroupId == 0)
                    boosterActionContext.SetGroupId(Context.NextBoosterGroupId());
                
                queue.Enqueue(boosterActionContext);
            }
        }
        
        private void ApplyMarkedActions(IGridModel model, IGridView view, CellImpactMarkData[,] markData)
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
                        Context.ReleaseAndSetNull(obj, coord);
                        
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
                            Context.ReleaseAndSetNull(obj, coord);
                            
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
        }
    }
}