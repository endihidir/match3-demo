using Core.Item;
using Core.UI;
using UnityEngine;

namespace Core.Presenters
{
    public interface IGoalFxAnimationHandler
    {
        void QueueAnimation(IDamageableItem item, Vector3 worldPos, Vector2 size);
        void PlayQueuedAnimations();
    }
}