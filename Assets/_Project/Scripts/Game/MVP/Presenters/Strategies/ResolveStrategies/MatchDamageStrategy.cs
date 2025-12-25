using System.Collections.Generic;
using Core.Item;
using Core.Utils;
using UnityEngine;

namespace Core.Handlers
{
    public sealed class MatchDamageStrategy
    {
        public void Apply(CellResolveData[,] cells, int width, int height, List<Vector2Int> group)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var c = group[i];

                ref var cell = ref cells[c.x, c.y];
                
                cell.AddDamage(1, DamageSource.Item);

                ResolveMarkHelper.AddNeighborObstacleDamage(cells, width, height, c);
            }
        }
    }
}