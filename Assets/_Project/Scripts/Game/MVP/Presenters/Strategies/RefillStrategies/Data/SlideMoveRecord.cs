using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct SlideMoveRecord
    {
        public Vector2Int[] ItemCoordPath;
    }
    
    public readonly struct SlideMovePlan
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        public readonly BaseGridObject Item;
        public readonly bool HasSlide;

        public SlideMovePlan(Vector2Int from, Vector2Int to, BaseGridObject item, bool hasSlide)
        {
            From = from;
            To = to;
            Item = item;
            HasSlide = hasSlide;
        }
    }
}