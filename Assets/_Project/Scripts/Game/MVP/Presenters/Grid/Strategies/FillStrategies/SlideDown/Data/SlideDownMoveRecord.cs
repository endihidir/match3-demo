using Game.Grid.Item;
using UnityEngine;

namespace  Game.Grid.Strategies.Data
{
    public struct SlideDownMoveRecord
    {
        public readonly BaseGridObject Item;
        public int HeadNode;
        public int TailNode;
        public int PathCount;
        public Vector2Int FinalCoord;
        public bool IsSpawn;
        public bool IsSlide;

        public SlideDownMoveRecord(BaseGridObject item)
        {
            Item = item;
            HeadNode = -1;
            TailNode = -1;
            PathCount = 0;
            FinalCoord = default;
            IsSpawn = false;
            IsSlide = false;
        }
    }
}