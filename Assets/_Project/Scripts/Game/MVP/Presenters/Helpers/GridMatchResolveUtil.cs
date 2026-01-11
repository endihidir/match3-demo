using System.Collections.Generic;
using Core.Item;
using Core.Models;
using UnityEngine;

namespace Core.Utils
{
    public static class GridMatchResolveUtil
    {
        public static BaseGridObject[] GetMergedGroupObject(List<Vector2Int> group, IGridModel model, Vector2Int centerCoord)
        {
            var mergeObjs = new BaseGridObject[group.Count - 1];
            var idx = 0;

            for (int i = 0; i < group.Count; i++)
            {
                var coord = group[i];
                if (coord == centerCoord) continue;
                var obj = model.GetGridObject(coord);
                if (!obj) continue;
                mergeObjs[idx++] = obj;
            }

            return mergeObjs;
        }
        
        public static void SetNullMergedObjectCoords(IGridModel model, List<Vector2Int> group, Vector2Int centerCoord)
        {
            for (int i = 0; i < group.Count; i++)
            {
                var coord = group[i];
                if (coord == centerCoord) continue;
                model.SetGridObject(coord, null);
            }
        }
    }
}