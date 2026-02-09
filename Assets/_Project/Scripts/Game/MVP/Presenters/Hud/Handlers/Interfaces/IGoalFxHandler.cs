using Core.Item;
using UnityEngine;

namespace Core.Presenters
{
    public interface IGoalFxHandler
    {
        void QueueFX(ObstacleType obstacleType, Vector3 worldPos, Vector2 size);
        void PlayQueuedFX();
    }
}