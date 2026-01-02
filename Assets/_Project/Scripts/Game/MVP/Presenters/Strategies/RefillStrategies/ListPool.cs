using System.Collections.Generic;
using UnityEngine;

namespace Core.Handlers
{
    public static class ListPool
    {
        private static readonly Stack<List<Vector2Int>> Pool = new(128);

        public static List<Vector2Int> Get()
        {
            if (Pool.Count > 0)
            {
                var list = Pool.Pop();
                list.Clear();
                return list;
            }

            return new List<Vector2Int>(8);
        }

        public static void Release(List<Vector2Int> list)
        {
            if (list == null) return;

            list.Clear();
            Pool.Push(list);
        }
    }
}