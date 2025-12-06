using UnityEngine;

namespace Core.Item
{
    public interface IItemObjectWriter
    {
        void SetGridPos(Vector2Int gridPos);
        void SetSize(Vector2 size);
    }
}