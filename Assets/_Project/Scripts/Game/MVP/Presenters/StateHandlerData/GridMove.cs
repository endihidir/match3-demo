using UnityEngine;

namespace Core.Handlers
{
    public readonly struct GridMove
    {
        public readonly GridMoveType Type;
        public readonly Vector2Int CoordA;
        public readonly Vector2Int CoordB;

        public GridMove(GridMoveType type, Vector2Int coordA, Vector2Int coordB = default)
        {
            Type = type;
            CoordA = coordA;
            CoordB = coordB;
        }
    }
}