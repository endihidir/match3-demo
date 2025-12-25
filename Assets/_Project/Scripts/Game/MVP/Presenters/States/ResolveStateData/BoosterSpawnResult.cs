using System.Collections.Generic;
using Core.Item;
using UnityEngine;

namespace Core.Handlers
{
    public struct BoosterSpawnResult
    {
        public readonly bool HasSpawn;
        public readonly Vector2Int Pos;
        public readonly BoosterType Type;
        public readonly List<Vector2Int> GroupCells;

        public BoosterSpawnResult(bool hasSpawn, Vector2Int pos, BoosterType type, List<Vector2Int> groupCells)
        {
            HasSpawn = hasSpawn;
            Pos = pos;
            Type = type;
            GroupCells = groupCells;
        }
    }
}