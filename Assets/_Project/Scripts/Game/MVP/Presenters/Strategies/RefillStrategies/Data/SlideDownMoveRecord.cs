using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct SlideDownMoveRecord
    {
        public readonly BaseGridObject Item;
        public readonly List<Vector2Int> Path;
        public readonly Vector2Int Final;
        public readonly bool IsSpawn;
        public readonly bool IsSlide;

        public SlideDownMoveRecord(BaseGridObject item, List<Vector2Int> path, Vector2Int final, bool isSpawn, bool isSlide)
        {
            Item = item;
            Path = path;
            Final = final;
            IsSpawn = isSpawn;
            IsSlide = isSlide;
        }
    }
}