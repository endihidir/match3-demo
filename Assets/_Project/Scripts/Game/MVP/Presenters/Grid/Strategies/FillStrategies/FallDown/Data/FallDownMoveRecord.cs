using Game.Grid.Item;
using UnityEngine;

namespace Game.Grid.Strategies.Data
{
    public readonly struct FallDownMoveRecord
    {
        public readonly BaseGridObject Item;
        public readonly Vector2Int FinalCoord;
        public readonly bool IsSpawn;

        public FallDownMoveRecord(BaseGridObject item, Vector2Int finalCoord, bool isSpawn)
        {
            Item = item;
            FinalCoord = finalCoord;
            IsSpawn = isSpawn;
        }
    }
}