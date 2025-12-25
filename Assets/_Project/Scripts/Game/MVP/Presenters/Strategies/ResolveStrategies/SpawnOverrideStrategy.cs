using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class SpawnOverrideStrategy
    {
        public void Apply(CellResolveData[,] cells, List<Vector2Int> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                ref var cell = ref cells[c.x, c.y];
                cell.Remove = false;
                cell.Source &= ~DamageSource.Item;
            }
        }
    }
}