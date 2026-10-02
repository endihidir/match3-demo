using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Strategies.Data
{
    public readonly struct FillSpawnRequest
    {
        public readonly BaseGridObject Item;
        public readonly int Column;
        public readonly int StackIndex;
        public readonly Vector2Int Target;

        public FillSpawnRequest(BaseGridObject item, int column, int stackIndex, Vector2Int target)
        {
            Item = item;
            Column = column;
            StackIndex = stackIndex;
            Target = target;
        }
    }
}