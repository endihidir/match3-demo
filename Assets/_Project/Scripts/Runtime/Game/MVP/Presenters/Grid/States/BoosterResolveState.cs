using System;
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
using Game.Level.Handlers;

namespace Game.Grid.States
{
    public sealed class BoosterResolveState : StateBase<GridStateContext>
    {
        public override bool NeedsExitPermission => true;

        private readonly IGridObjectDestroyHandler _gridObjectDestroyHandler;
        private readonly ILevelGoalHandler _levelGoalHandler;
        private readonly IBoosterFxHandler _boosterFxHandler;

        private readonly HashSet<BoosterActionKey> _processedBoosters = new();
        private readonly HashSet<Vector2Int> _protectedMatchGroups = new();
        private readonly List<BaseGridObject> _hitResultBuffer = new();

        private bool[,] _visitedBuffer;
        private Vector2Int[] _coordBuffer;

        public BoosterResolveState(GridStateContext context, IBoosterFxHandler boosterFxHandler, IGridObjectDestroyHandler objectDestroyHandler, ILevelGoalHandler goalHandler) : base(context)
        {
            _boosterFxHandler = boosterFxHandler;
            _gridObjectDestroyHandler = objectDestroyHandler;
            _levelGoalHandler = goalHandler;
        }

        protected override void OnEnter()
        {
            ClearBuffers();
            CollectProtectedMatchGroup();
            ProcessAllBoosters();
            RequestExit();
        }

        private void ProcessAllBoosters()
        {
            while (Context.PendingBoosterActions.Count > 0)
            {
                var action = Context.PendingBoosterActions[0];
                Context.PendingBoosterActions.RemoveAt(0);

                var key = new BoosterActionKey(action.OriginCoord, action.BoosterAction);
                if (!_processedBoosters.Add(key)) continue;

                ExecuteBooster(action);
            }

            Context.RaiseDestructionStateComplete();
        }

        private void ExecuteBooster(BoosterActionContext action)
        {
            var model = Context.GridModel;
            var view = Context.GridView;

            _boosterFxHandler.PlayBoosterFxAsync(action, out var animSpeed).Forget();
            var timeline = BoosterTimelineBuilder.BuildTimeline(action, model, view, animSpeed);
            timeline.SortByDelayThenCoord();

            var entries = timeline.Entries;
            var damageAmount = action.BoosterAction.DamageAmount;

            _hitResultBuffer.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                var impactedObj = ApplyLogicImpact(entries[i].Coord, damageAmount);
                _hitResultBuffer.Add(impactedObj);
            }

            PlayTimelineVisualsAsync(entries, _hitResultBuffer.ToArray()).Forget();
        }
        
        private BaseGridObject ApplyLogicImpact(Vector2Int coord, int damageAmount)
        {
            if (_protectedMatchGroups.Contains(coord)) return null;

            var model = Context.GridModel;
            var obj = model.GetGridObject(coord);
            if (!obj) return null;

            if (obj is IBoosterActionSource source && source.TryBuildAction(coord, out var newAction))
            {
                Context.PendingBoosterActions.Add(newAction);
                _gridObjectDestroyHandler.SetNullCoord(obj);
                return obj;
            }

            if (obj is IDamageableGridObject damageable)
            {
                var result = damageable.ApplyDamageLogic(damageAmount, GridDamageSource.Booster);

                if (result == GridDamageResult.Ignored) return null;

                if (result == GridDamageResult.Destroyed)
                {
                    _levelGoalHandler.ProgressGoal(obj);
                    _gridObjectDestroyHandler.SetNullCoord(obj);
                }

                return obj;
            }

            _gridObjectDestroyHandler.SetNullCoord(obj);
            return obj;
        }
        
        private async UniTask PlayTimelineVisualsAsync(IReadOnlyList<BoosterImpactEntry> entries, BaseGridObject[] pendingRelease)
        {
            var lastDelay = 0f;
            var model = Context.GridModel;

            for (int i = 0; i < entries.Count; i++)
            {
                var waitTime = entries[i].Delay - lastDelay;

                if (waitTime > 0f)
                    await UniTask.WaitForSeconds(waitTime);

                lastDelay = entries[i].Delay;

                var obj = pendingRelease[i];
                if (!obj) continue;

                if (obj is IDamageableGridObject damageable)
                {
                    damageable.ApplyDamageVisual();

                    if (model.GetGridObject(entries[i].Coord) != obj)
                    {
                        _gridObjectDestroyHandler.PlayBlastFx(obj);
                        _gridObjectDestroyHandler.ReleaseObject(obj);
                    }
                }
                else
                {
                    _gridObjectDestroyHandler.PlayBlastFx(obj);
                    _gridObjectDestroyHandler.ReleaseObject(obj);
                }
            }
        }

        private void CollectProtectedMatchGroup()
        {
            if (!Context.ProtectedCoord.HasValue) return;

            var protectedCoord = Context.ProtectedCoord.Value;
            Context.ProtectedCoord = null;

            var model = Context.GridModel;
            var grid = model.BuildGridTypeData();
            var data = grid[protectedCoord.x, protectedCoord.y];

            if (!GridMatchCalcUtil.IsRegularItem(data)) return;

            if (_visitedBuffer == null)
            {
                _visitedBuffer = new bool[model.Width, model.Height];
                _coordBuffer = new Vector2Int[model.Width * model.Height];
            }
            else
            {
                Array.Clear(_visitedBuffer, 0, _visitedBuffer.Length);
            }

            var count = GridMatchCalcUtil.CollectMatchShapeFromCenter(
                model, grid, protectedCoord.x, protectedCoord.y, data.TypeId, _visitedBuffer, _coordBuffer);

            if (count < 4) return;

            for (int i = 0; i < count; i++)
                _protectedMatchGroups.Add(_coordBuffer[i]);
        }

        private void ClearBuffers()
        {
            _processedBoosters.Clear();
            _protectedMatchGroups.Clear();
        }
    }
}