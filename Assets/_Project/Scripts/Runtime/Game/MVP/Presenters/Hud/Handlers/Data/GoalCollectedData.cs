using Game.Configs;
using Game.Grid.Item;
using UnityEngine;

namespace Game.HUD.Handlers.Data
{
    public readonly struct GoalCollectedData
    {
        public readonly GridObjectType GridObjectType;
        public readonly BaseGridObjectDataSO GridObjectData;
        public readonly Vector3 WorldPos;
        public readonly Vector2 RectSize;

        public GoalCollectedData(GridObjectType objectType, BaseGridObjectDataSO gridObjectData, Vector3 worldPos, Vector2 rectSize)
        {
            GridObjectType = objectType;
            GridObjectData = gridObjectData;
            WorldPos = worldPos;
            RectSize = rectSize;
        }
    }
}