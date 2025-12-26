using System.Collections.Generic;
using Core.Item;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class MatchResolveHandler : IMatchResolveHandler
    {
        private IBoosterSelectionHandler _boosterSelection;

        public MatchResolveHandler(IBoosterSelectionHandler boosterSelectionHandler)
        {
            _boosterSelection = boosterSelectionHandler;
        }

        public void Initialize(GridStateContext context)
        {
            _boosterSelection.Initialize(context);
        }

        public void Handle(GridStateContext context, GridObjectType[,] typeGrid, List<Vector2Int> group, int width, int height, CellResolveData[,] cells,
            List<BoosterSpawnResult> spawns)
        {
            if (group == null || group.Count == 0) return;

            var id = typeGrid[group[0].x, group[0].y].TypeId;

            var spawn = _boosterSelection.Decide(typeGrid, group, width, height, id);

            ApplyMatchImpact(cells, width, height, group);

            if (spawn.HasSpawn)
            {
                spawns.Add(spawn);
                ClearItemRemovalFromCells(cells, group);
            }
        }

        private void ApplyMatchImpact(CellResolveData[,] cells, int width, int height, List<Vector2Int> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                ref var cell = ref cells[c.x, c.y];

                cell.MarkRemove(DamageSource.Item);

                ResolveMarkHelper.AddNeighborObstacleDamage(cells, width, height, c);
            }
        }

        private void ClearItemRemovalFromCells(CellResolveData[,] cells, List<Vector2Int> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                ref var cell = ref cells[c.x, c.y];

                cell.ClearRemoveFlag(DamageSource.Item);
            }
        }
    }
}