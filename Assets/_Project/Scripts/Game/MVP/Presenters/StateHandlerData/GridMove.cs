using UnityEngine;

namespace Core.Handlers
{
    public readonly struct GridMove
    {
        public readonly GridMoveType MoveType;
        public readonly Vector2Int SourceCoord;
        public readonly Vector2Int TargetCoord;

        public GridMove(GridMoveType moveType, Vector2Int sourceCoord, Vector2Int targetCoord = default)
        {
            MoveType = moveType;
            SourceCoord = sourceCoord;
            TargetCoord = targetCoord;
        }
    }
}