using System.Collections.Generic;
using Game.Grid.Item;
using Game.Models;
using UnityEngine;

namespace Game.Grid.Handlers
{
    public interface IMatchDestructionHandler
    {
        void DestroyGroup(List<Vector2Int> group);
        void ClearGroupForMerge(List<Vector2Int> group);
        void ReleaseObjects(BaseGridObject[] objects);
        void ReleaseObject(BaseGridObject obj);
    }
}
