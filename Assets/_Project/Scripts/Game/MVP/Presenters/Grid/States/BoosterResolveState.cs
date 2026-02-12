using System.Collections.Generic;
using Core.Configs;
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
        private readonly IBoosterFxHandler _boosterFxHandler;

        public BoosterResolveState(GridStateContext context, IBoosterFxHandler boosterFxHandler) : base(context)
        {
            _boosterFxHandler = boosterFxHandler;
        }

        protected override void OnEnter()
        {
            ApplyPendingActions();
            RequestExit();
        }
        
        private void ApplyPendingActions()
        {
            var model = Context.GridModel;
            var view = Context.GridView;
            MarkPendingActions(model, view, out var markData);
            ApplyMarkedActions(model, markData);
        }

        private void MarkPendingActions(IGridModel model, IGridView view, out BoosterImpactRecord[,] markData)
        {
            markData = new BoosterImpactRecord[model.Width, model.Height]; 
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
                        BoosterImpactResolver.ResolveLinearArea(model, action, markData, hAction.DamageAmount, hAction.LineCount, horDirs, EnqueueIfNew);
                        break;
                    case RocketVerticalAction vAction:
                        var verDirs = DirectionLookup.VerticalDirections;
                        BoosterImpactResolver.ResolveLinearArea(model, action, markData, vAction.DamageAmount, vAction.LineCount, verDirs, EnqueueIfNew);
                        break;
                    case BombAction bAction:
                        BoosterImpactResolver.ResolveSquareArea(model, action, markData, bAction.DamageAmount, bAction.Radius, EnqueueIfNew);
                        break;
                }
            }

            if (!Context.HasUnmarkRemoveRequested) return;
            
            Context.HasUnmarkRemoveRequested = false;
                
            var unmarkRemoveCoord = Context.UnmarkRemoveCoord;

            if (!GridMatchMaskBuilder.TryBuildMatchMask(model, out var matchMask)) return;
                
            if (!GridMatchCalcUtil.TryBuildMatchGroupMaskAt(model, unmarkRemoveCoord, matchMask, out var groupMask)) return;
                    
            for (int x = 0; x < model.Width; x++)
            {
                for (int y = 0; y < model.Height; y++)
                {
                    if (!groupMask[x, y]) continue;
                                
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
                
                _boosterFxHandler.PlayBoosterFx(boosterActionContext.BoosterAction, boosterActionContext.OriginCoord, model, view);
                
                queue.Enqueue(boosterActionContext);
            }
        }
        
        private void ApplyMarkedActions(IGridModel model, BoosterImpactRecord[,] markData)
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
                        continue;
                    }

                    if (data.HasDamage && obj is IDamageableGridObject damageableItem)
                    {
                        var damageResult = damageableItem.TakeDamage(data.DamageAmount, data.GridDamageSource);

                        if (damageResult == GridDamageResult.Destroyed)
                        {
                            Context.ProgressGoal(damageableItem, obj.Coord, obj.SpriteRenderer.size);
                            Context.ReleaseAndSetNull(obj, coord);
                        }
                        continue;
                    }
                }
            }
            
            Context.RaiseObjectsDestroyed();
        }
    }
}