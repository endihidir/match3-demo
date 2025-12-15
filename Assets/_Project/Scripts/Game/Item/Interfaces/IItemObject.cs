using UnityEngine;

namespace Core.Item
{
    public interface IItemObject
    {
        Transform Transform { get; }
        Vector2Int Coordinate { get; }
        SpriteRenderer SpriteRenderer { get; }
        ItemAnimation ItemAnimation { get; }
    }
}