using UnityEngine;

namespace Core.Item
{
    public interface IItemObjectReader
    {
        Vector2Int Coordinate { get; }
        SpriteRenderer SpriteRenderer { get; }
        Transform Transform { get; }
    }
}