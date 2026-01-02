using UnityEngine;

namespace Core.Handlers
{
    public readonly struct InputSource
    {
        public readonly GridInputType InputType;
        public readonly Vector2Int SourceCoord;
        public readonly Vector2Int TargetCoord;

        public InputSource(GridInputType inputType, Vector2Int sourceCoord, Vector2Int targetCoord = default)
        {
            InputType = inputType;
            SourceCoord = sourceCoord;
            TargetCoord = targetCoord;
        }
    }
}