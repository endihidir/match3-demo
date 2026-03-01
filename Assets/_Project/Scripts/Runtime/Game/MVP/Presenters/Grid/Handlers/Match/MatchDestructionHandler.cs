using System.Collections.Generic;
using Game.Grid.Item;
using Game.Level.Handlers;
using Game.Models;
using Game.Utils;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public sealed class MatchDestructionHandler : IMatchDestructionHandler
    {
        private readonly IGridModel _gridModel;
        private readonly IGridObjectDestroyHandler _destroyHandler;
        private readonly ILevelGoalHandler _goalHandler;

        public MatchDestructionHandler(IGridModel gridModel, IGridObjectDestroyHandler destroyHandler, ILevelGoalHandler goalHandler)
        {
            _gridModel = gridModel;
            _destroyHandler = destroyHandler;
            _goalHandler = goalHandler;
        }

        public void DestroyGroup(List<Vector2Int> group)
        {
            foreach (var coord in group)
            {
                var obj = _gridModel.GetGridObject(coord);
                if (!obj) continue;

                ApplyNeighbourDamage(coord);
                _destroyHandler.DestroyGridObject(obj);
            }
        }

        public void ClearGroupForMerge(List<Vector2Int> group)
        {
            foreach (var coord in group)
            {
                ApplyNeighbourDamage(coord);
                _gridModel.SetGridObject(coord, null);
            }
        }

        public void ReleaseObjects(BaseGridObject[] objects)
        {
            foreach (var obj in objects)
            {
                if (!obj) continue;
                _destroyHandler.ReleaseObject(obj);
            }
        }

        private void ApplyNeighbourDamage(Vector2Int origin)
        {
            foreach (var dir in DirectionLookup.LinearDirections)
            {
                if (!_gridModel.TryGetNeighbourCoord(origin, dir, out var neighbourCoord)) continue;

                var obj = _gridModel.GetGridObject(neighbourCoord);
                if (!obj) continue;

                if (obj is not IDamageableGridObject damageable) continue;

                var result = damageable.TakeDamage(1, GridDamageSource.Match);

                if (result == GridDamageResult.Destroyed)
                {
                    _goalHandler.ProgressGoal(obj);
                    _destroyHandler.DestroyGridObject(obj);
                }
            }
        }
    }
}
