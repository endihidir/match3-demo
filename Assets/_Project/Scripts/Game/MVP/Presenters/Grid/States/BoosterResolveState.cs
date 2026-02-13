using System.Collections.Generic;
using Core.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Handlers
{
    public class BoosterResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        private readonly IBoosterFxHandler _boosterFxHandler;
        private readonly HashSet<Vector2Int> _impactedCells = new();
        private readonly HashSet<BoosterActionKey> _processedBoosters = new();
        private readonly HashSet<Vector2Int> _unmarkProtectedCells = new();
        private readonly List<UniTask> _activeTasks = new();

        public BoosterResolveState(GridStateContext context, IBoosterFxHandler boosterFxHandler) : base(context)
        {
            _boosterFxHandler = boosterFxHandler;
        }

        protected override void OnEnter()
        {
            _impactedCells.Clear();
            _processedBoosters.Clear();
            _activeTasks.Clear();
            _unmarkProtectedCells.Clear();

            BuildUnmarkProtectedCells();
            ProcessBoostersAsync().Forget();
        }

        private async UniTaskVoid ProcessBoostersAsync()
        {
            while (Context.PendingBoosterActions.Count > 0 || _activeTasks.Count > 0)
            {
                while (Context.PendingBoosterActions.Count > 0)
                {
                    var action = Context.PendingBoosterActions[0];
                    Context.PendingBoosterActions.RemoveAt(0);

                    var key = new BoosterActionKey(action.OriginCoord, action.BoosterAction);
                    if (!_processedBoosters.Add(key)) continue;

                    _activeTasks.Add(ExecuteBoosterAsync(action));
                }

                _activeTasks.RemoveAll(t => t.Status.IsCompleted());
                await UniTask.Yield();
            }
            
            Context.RaiseDestructionStateComplete();
            RequestExit();
        }

        private async UniTask ExecuteBoosterAsync(BoosterActionContext action)
        {
            var model = Context.GridModel;
            var view = Context.GridView;
            var fxCompleted = false;
            
            var animSpeed = _boosterFxHandler.PlayBoosterFx(action, view, () => fxCompleted = true);
            var timeline = BoosterImpactResolver.BuildTimeline(action, model, view, animSpeed);
            await ProcessTimelineAsync(timeline, action.BoosterAction.DamageAmount);
            await UniTask.WaitUntil(() => fxCompleted);
        }

        private async UniTask ProcessTimelineAsync(ImpactTimeline timeline, int damageAmount)
        {
            timeline.SortByDelay();
            var lastDelay = 0f;

            foreach (var entry in timeline.Entries)
            {
                var waitTime = entry.Delay - lastDelay;
                
                if (waitTime > 0)
                    await UniTask.WaitForSeconds(waitTime);

                lastDelay = entry.Delay;
                ApplyImpact(entry.Coord, damageAmount);
            }
            
            Context.RaiseObjectsDestroyed();
        }

        private void ApplyImpact(Vector2Int coord, int damageAmount)
        {
            if (_unmarkProtectedCells.Contains(coord)) return;
            if (!_impactedCells.Add(coord)) return;

            var model = Context.GridModel;
            var obj = model.GetGridObject(coord);
            
            if (!obj) return;

            if (obj is IBoosterActionSource source && source.TryBuildAction(coord, out var newAction))
            {
                newAction.SetGroupId(Context.NextBoosterGroupId());
                Context.PendingBoosterActions.Add(newAction);
                Context.ReleaseAndSetNull(obj, coord);
                return;
            }

            if (obj is IDamageableGridObject damageable)
            {
                var result = damageable.TakeDamage(damageAmount, GridDamageSource.Booster);

                if (result == GridDamageResult.Destroyed)
                {
                    Context.ProgressGoal(damageable, coord, obj.SpriteRenderer.size);
                    Context.ReleaseAndSetNull(obj, coord);
                }
            }
            else
            {
                Context.ReleaseAndSetNull(obj, coord);
            }
        }
        
        private void BuildUnmarkProtectedCells()
        {
            if (!Context.HasUnmarkRemoveRequested) return;

            Context.HasUnmarkRemoveRequested = false;

            var model = Context.GridModel;
            var unmarkRemoveCoord = Context.UnmarkRemoveCoord;

            if (!GridMatchMaskBuilder.TryBuildMatchMask(model, out var matchMask)) return;
            if (!GridMatchCalcUtil.TryBuildMatchGroupMaskAt(model, unmarkRemoveCoord, matchMask, out var groupMask)) return;

            for (int x = 0; x < model.Width; x++)
            {
                for (int y = 0; y < model.Height; y++)
                {
                    if (!groupMask[x, y]) continue;
                    _unmarkProtectedCells.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}