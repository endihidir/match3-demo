using UnityEngine;

namespace Core.Handlers
{
    public interface IGridStateHandler
    {
        bool TryEnqueueInput(Vector2Int sourceCoord, Vector2Int direction);
    }
}