using Core.Handlers;
using UnityEngine;

namespace Core.Item
{
    public interface IBoosterActionSource
    {
        bool TryBuildAction(Vector2Int origin, out BoosterActionContext boosterActionContext);
    }
}