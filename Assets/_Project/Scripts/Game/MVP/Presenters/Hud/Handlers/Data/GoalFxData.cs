using Core.Item;
using Core.UI;
using UnityEngine;

namespace Core.Presenters
{
    public readonly struct GoalFxData
    {
        public readonly GoalFxView FxView;
        public readonly ObstacleType ObstacleType;
        public readonly Vector3 TargetWorldPos;
        public readonly Vector2 TargetSize;
    
        public GoalFxData(GoalFxView fxView, ObstacleType obstacleType, Vector3 targetWorldPos, Vector2 targetSize)
        {
            ObstacleType = obstacleType;
            FxView = fxView;
            TargetWorldPos = targetWorldPos;
            TargetSize = targetSize;
        }
    }
}