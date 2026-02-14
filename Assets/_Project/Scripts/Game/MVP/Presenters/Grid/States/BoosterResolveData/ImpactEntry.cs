using UnityEngine;

namespace Core.Handlers
{
    public readonly struct ImpactEntry
    {
        public readonly Vector2Int Coord;
        public readonly float Delay;

        public ImpactEntry(Vector2Int coord, float delay)
        {
            Coord = coord;
            Delay = delay;
        }
    }
}