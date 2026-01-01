using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct SlideMovePlan
    {
        public readonly Vector2Int From;
        public readonly Vector2Int To;
        public readonly BaseGridObject Item;
        public readonly bool IsSlide;

        public SlideMovePlan(Vector2Int from, Vector2Int to, BaseGridObject item, bool isSlide)
        {
            From = from;
            To = to;
            Item = item;
            IsSlide = isSlide;
        }
    }
}