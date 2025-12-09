using UnityEngine;

namespace Core.Utils
{
    public static class CoordinateUtils
    {
        public static int ToIndex(Vector2Int coordinate, int width) => coordinate.y * width + coordinate.x;
        public static Vector2Int ToCoordinate(int index, int width) => new (index % width, index / width);
    }
}