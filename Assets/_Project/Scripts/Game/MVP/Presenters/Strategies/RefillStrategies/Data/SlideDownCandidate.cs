using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct SlideDownCandidate
    {
        public readonly BaseGridObject Item;
        public readonly Vector2Int From;
        public readonly Vector2Int To;

        public SlideDownCandidate(BaseGridObject item, Vector2Int from, Vector2Int to)
        {
            Item = item;
            From = from;
            To = to;
        }
    }
}