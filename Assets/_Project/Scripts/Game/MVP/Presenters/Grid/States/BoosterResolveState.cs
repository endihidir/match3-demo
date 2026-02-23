using System.Collections.Generic;
using Game.Grid.Contexts;
using Game.Grid.Item;
using Core.StateMachineCore;
using Core.Utils;
using Cysharp.Threading.Tasks;
using Game.Grid.Handlers;
using UnityEngine;
using Game.Grid.States.Data;
using Game.Grid.Utils;

namespace Game.Grid.States
{
    public sealed class BoosterResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;
        private readonly IGridObjectDestroyHandler _gridObjectDestroyHandler;
        private readonly ILevelGoalProgressHandler _levelGoalProgressHandler;

        private readonly IBoosterFxHandler _boosterFxHandler;
        private readonly HashSet<BoosterActionKey> _processedBoosters = new();
        private readonly HashSet<Vector2Int> _unmarkProtectedCells = new();
        private readonly List<UniTask> _activeTasks = new();

        public BoosterResolveState(GridStateContext context, IBoosterFxHandler boosterFxHandler, IGridObjectDestroyHandler objectDestroyHandler, ILevelGoalProgressHandler goalProgressHandler) : base(context)
        {
            _boosterFxHandler = boosterFxHandler;
            _gridObjectDestroyHandler = objectDestroyHandler;
            _levelGoalProgressHandler = goalProgressHandler;
        }

        protected override void OnEnter()
        {
            ClearBuffers();
            BuildUnmarkProtectedCells();
            ProcessBoostersAsync().Forget();
        }

        private async UniTask ProcessBoostersAsync()
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
            
            _boosterFxHandler.PlayBoosterFxAsync(action, out var animSpeed).Forget();
            var timeline = BoosterTimelineBuilder.BuildTimeline(action, model, view, animSpeed);
            var timelineTask = ProcessTimelineAsync(timeline, action.BoosterAction.DamageAmount);
            await UniTask.WhenAll(timelineTask);
        }

        private async UniTask ProcessTimelineAsync(ImpactTimeline timeline, int damageAmount)
        {
            timeline.SortByDelayThenCoord();
            var lastDelay = 0f;

            foreach (var entry in timeline.Entries)
            {
                var waitTime = entry.Delay - lastDelay;
                
                if(waitTime > 0f)
                    await UniTask.WaitForSeconds(waitTime);
                
                lastDelay = entry.Delay;
                ApplyImpact(entry.Coord, damageAmount);
            }
        }

        private void ApplyImpact(Vector2Int coord, int damageAmount)
        {
            if (_unmarkProtectedCells.Contains(coord)) return;

            var model = Context.GridModel;
            var obj = model.GetGridObject(coord);
            if (!obj) return;

            if (obj is IBoosterActionSource source && source.TryBuildAction(coord, out var newAction))
            {
                Context.PendingBoosterActions.Add(newAction);
                _gridObjectDestroyHandler.DestroyGridObject(obj, coord);
                return;
            }

            if (obj is IDamageableGridObject damageable)
            {
                var result = damageable.TakeDamage(damageAmount, GridDamageSource.Booster);

                if (result == GridDamageResult.Destroyed)
                {
                    _levelGoalProgressHandler.ProgressGoal(damageable, coord, obj.SpriteRenderer.size);
                    _gridObjectDestroyHandler.DestroyGridObject(obj, coord);
                }
            }
            else
            {
                _gridObjectDestroyHandler.DestroyGridObject(obj, coord);
            }
        }
        
        private void BuildUnmarkProtectedCells()
        {
            if (!Context.ProtectedCoord.HasValue) return;

            var protectedCoord = Context.ProtectedCoord.Value;
            Context.ProtectedCoord = null;

            var model = Context.GridModel;
            var grid = model.BuildGridTypeData();
            var data = grid[protectedCoord.x, protectedCoord.y];

            if (!GridMatchCalcUtil.IsRegularItem(data)) return;

            var visited = new bool[model.Width, model.Height];
            var buffer = new Vector2Int[model.Width * model.Height];

            var count = GridMatchCalcUtil.CollectMatchShapeFromCenter(model, grid, protectedCoord.x, protectedCoord.y, data.TypeId, visited, buffer);

            for (int i = 0; i < count; i++)
                _unmarkProtectedCells.Add(buffer[i]);
        }
        
        private void ClearBuffers()
        {
            _processedBoosters.Clear();
            _activeTasks.Clear();
            _unmarkProtectedCells.Clear();
        }
    }
}