using UnityEngine;

namespace Game.Grid.States.Data
{
    public readonly struct BoosterImpactEntry
    {
        public readonly Vector2Int Coord;
        public readonly float Delay;

        public BoosterImpactEntry(Vector2Int coord, float delay)
        {
            Coord = coord;
            Delay = delay;
        }
    }
}