using Core.Handlers;
using UnityEngine;

namespace Core.Item
{
    public interface ITriggerEffectSource
    {
        bool TryBuildEffect(Vector2Int origin, out PendingEffect effect);
    }
}