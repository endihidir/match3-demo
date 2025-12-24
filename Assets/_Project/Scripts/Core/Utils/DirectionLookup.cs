using UnityEngine;

namespace Core.Utils
{
    public static class DirectionLookup
    {
        public static readonly Vector2Int[] LinearDirections =
        {
            new (0, 1),
            new (1, 0),
            new (-1, 0),
            new (0, -1)
        };

        public static readonly Vector2Int[] DiagonalDirections =
        {
            new (1, 1),
            new (-1, 1),
            new (1, -1),
            new (-1, -1)
        };

        public static readonly Vector2Int[] AllDirections =
        {
            new (0, 1),
            new (1, 0),
            new (-1, 0),
            new (0, -1),
            new (1, 1),
            new (-1, 1),
            new (1, -1),
            new (-1, -1)
        };
    }
}