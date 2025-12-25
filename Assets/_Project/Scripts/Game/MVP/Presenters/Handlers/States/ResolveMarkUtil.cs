using System.Collections.Generic;
using Core.Handlers;
using Core.Item;
using UnityEngine;

namespace Core.Utils
{
    public static class ResolveMarkUtil
    {
        public static List<Vector2Int> CollectGroup(bool[,] matchMask, bool[,] visited, GridObjectType[,] typeGrid, int width, int height, Vector2Int start, int id)
        {
            var result = new List<Vector2Int>(16);
            var q = new Queue<Vector2Int>(16);

            visited[start.x, start.y] = true;
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                result.Add(p);

                TryEnqueue(new Vector2Int(p.x - 1, p.y));
                TryEnqueue(new Vector2Int(p.x + 1, p.y));
                TryEnqueue(new Vector2Int(p.x, p.y - 1));
                TryEnqueue(new Vector2Int(p.x, p.y + 1));
            }

            return result;

            void TryEnqueue(Vector2Int n)
            {
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) return;
                if (!matchMask[n.x, n.y]) return;
                if (visited[n.x, n.y]) return;

                var d = typeGrid[n.x, n.y];
                if (!GridMatchDetectUtil.IsRegularItem(d)) return;
                if (d.TypeId != id) return;

                visited[n.x, n.y] = true;
                q.Enqueue(n);
            }
        }

        public static void AddNeighborObstacleDamage(CellResolveData[,] cells, int width, int height, Vector2Int c)
        {
            MarkNeighbour(c.x - 1, c.y);
            MarkNeighbour(c.x + 1, c.y);
            MarkNeighbour(c.x, c.y - 1);
            MarkNeighbour(c.x, c.y + 1);
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