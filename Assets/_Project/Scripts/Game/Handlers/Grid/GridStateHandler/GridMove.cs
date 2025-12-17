using UnityEngine;

namespace Core.Handlers
{
    public readonly struct GridMove
    {
        public readonly GridMoveType Type;
        public readonly Vector2Int A;
        public readonly Vector2Int B;

        public GridMove(GridMoveType type, Vector2Int a, Vector2Int b)
        {
            Type = type;
            A = a;
            B = b;
        }
    }
}