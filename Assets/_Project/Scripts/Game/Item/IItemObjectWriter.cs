using UnityEngine;

namespace Core.Item
{
    public interface IItemObjectWriter
    {
        void SetCoordinate(Vector2Int coordinate);
        void SetCellSize(Vector2 size);
    }
}