using UnityEngine;

namespace Core.Utils
{
    public static class CoordinateUtility
    {
        public static int ToIndex(Vector2Int pos, int width) => pos.y * width + pos.x;
        public static Vector2Int ToPos(int index, int width) => new (index % width, index / width);
    }
}