using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct MoveAnimRecord
    {
        public BaseGridObject Item;
        public List<Vector2Int> CoordPath;
        public bool IsSpawned;

        public bool HasPath => CoordPath != null && CoordPath.Count > 0;

        public MoveAnimRecord(BaseGridObject item, List<Vector2Int> coordPath, bool isSpawned)
        {
            Item = item;
            CoordPath = coordPath;
            IsSpawned = isSpawned;
        }
    }
}