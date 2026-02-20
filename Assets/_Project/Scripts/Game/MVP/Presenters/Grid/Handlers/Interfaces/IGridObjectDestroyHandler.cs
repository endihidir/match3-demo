using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IGridObjectDestroyHandler
    {
        void DestroyGridObject(BaseGridObject obj, Vector2Int coord);
        void SetNull(Vector2Int coord);                               
        void ReleaseObject(BaseGridObject obj);  
    }
}

