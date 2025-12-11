using UnityEngine;

namespace Core.Item
{
    public interface IItemObjectReader
    {
        Vector2Int Coordinate { get; }
        Transform Transform { get; }
    }
}