using UnityEngine;

namespace Core.Item
{
    public interface IItemObjectReader
    {
        Vector2Int GridPos { get; }
        SpriteRenderer SpriteRenderer { get; }
        Transform Transform { get; }
    }
}