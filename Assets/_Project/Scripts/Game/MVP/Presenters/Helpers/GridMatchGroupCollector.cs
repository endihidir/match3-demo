using System.Collections.Generic;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchGroupCollector
    { 
        public static List<Vector2Int> CollectGroupFromMask(IGridModel model, bool[,] matchMask, bool[,] visited, Vector2Int start)
        {
            var result = new List<Vector2Int>();
            var stack = new Stack<Vector2Int>();

            var startObj = model.GetGridObject(start.x, start.y);
            if (!startObj || !GridMatchRules.IsRegularItem(startObj.ObjectType)) return result;

            var typeId = startObj.TypeId;

            stack.Push(start);
            visited[start.x, start.y] = true;

            while (stack.Count > 0)
            {
                var p = stack.Pop();
                result.Add(p);

                foreach (var dir in DirectionLookup.LinearDirections)
                {
                    if (!model.TryGetNeighbourCoord(p, dir, out var n)) continue;
                    
                    TryPush(n.x, n.y);
                }
            }

            return result;

            void TryPush(int x, int y)
            {
                if (!model.IsInRange(x, y)) return;
                if (visited[x, y]) return;
                if (!matchMask[x, y]) return;

                var obj = model.GetGridObject(x, y);
                if (!obj) return;

                if (!GridMatchRules.IsRegularItem(obj.ObjectType)) return;
                if (obj.TypeId != typeId) return;

                visited[x, y] = true;
                stack.Push(new Vector2Int(x, y));
            }
        }
    }
}