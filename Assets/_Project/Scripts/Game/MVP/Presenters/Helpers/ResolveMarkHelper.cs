using System.Collections.Generic;
using Core.Handlers;
using Core.Item;
using UnityEngine;

namespace Core.Utils
{
    public static class ResolveMarkHelper
    {
        public static List<Vector2Int> CollectGroup(bool[,] matchMask, bool[,] visited, GridObjectType[,] typeGrid, int width, int height, Vector2Int start, int id)
        {
            var result = new List<Vector2Int>(16);
            var coords = new Queue<Vector2Int>(16);

            visited[start.x, start.y] = true;
            coords.Enqueue(start);

            while (coords.Count > 0)
            {
                var p = coords.Dequeue();
                result.Add(p);

                TryEnqueue(new Vector2Int(p.x - 1, p.y));
                TryEnqueue(new Vector2Int(p.x + 1, p.y));
                TryEnqueue(new Vector2Int(p.x, p.y - 1));
                TryEnqueue(new Vector2Int(p.x, p.y + 1));
            }

            return result;

            void TryEnqueue(Vector2Int coord)
            {
                if (coord.x < 0 || coord.y < 0 || coord.x >= width || coord.y >= height) return;
                if (!matchMask[coord.x, coord.y]) return;
                if (visited[coord.x, coord.y]) return;

                var gridObjectType = typeGrid[coord.x, coord.y];
                if (!GridMatchDetectUtil.IsRegularItem(gridObjectType)) return;
                if (gridObjectType.TypeId != id) return;

                visited[coord.x, coord.y] = true;
                coords.Enqueue(coord);
            }
        }

        public static void AddNeighborObstacleDamage(CellResolveData[,] cells, int width, int height, Vector2Int coord)
        {
            MarkNeighbour(coord.x - 1, coord.y);
            MarkNeighbour(coord.x + 1, coord.y);
            MarkNeighbour(coord.x, coord.y - 1);
            MarkNeighbour(coord.x, coord.y + 1);
            return;

            void MarkNeighbour(int x, int y)
            {
                if (x < 0 || y < 0 || x >= width || y >= height) return;
                ref var cell = ref cells[x, y];
                cell.AddObstacleOnlyDamage(1, DamageSource.Item);
            }
        }
    }
}