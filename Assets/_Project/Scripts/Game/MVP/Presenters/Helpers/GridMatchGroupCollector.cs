using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchGroupCollector
    {
        public static List<Vector2Int> CollectGroupFromMask(bool[,] matchMask, bool[,] visited, IGridModel model, Vector2Int coord, int id)
        {
            var grid = model.BuildTypeDataGrid();
            var group = new List<Vector2Int>(16);

            if (!model.IsInRange(coord)) return group;
            if (!matchMask[coord.x, coord.y]) return group;
            if (visited[coord.x, coord.y]) return group;
            if (!GridMatchRules.IsCellMatched(model, grid, coord.x, coord.y, id)) return group;

            var q = new Queue<Vector2Int>(16);
            q.Enqueue(coord);

            while (q.Count > 0)
            {
                var c = q.Dequeue();

                if (!model.IsInRange(c)) continue;
                if (visited[c.x, c.y]) continue;
                if (!matchMask[c.x, c.y]) continue;
                if (!GridMatchRules.IsCellMatched(model, grid, c.x, c.y, id)) continue;

                visited[c.x, c.y] = true;
                group.Add(c);

                q.Enqueue(new Vector2Int(c.x + 1, c.y));
                q.Enqueue(new Vector2Int(c.x - 1, c.y));
                q.Enqueue(new Vector2Int(c.x, c.y + 1));
                q.Enqueue(new Vector2Int(c.x, c.y - 1));
            }

            return group;
        }
    }
}