using Core.Configs;
using Core.Handlers;
using UnityEngine;

namespace Core.Item
{
    public interface IBoosterActionSource
    {
        BoosterActionBase BoosterAction { get; }
        bool TryBuildAction(Vector2Int origin, out BoosterActionContext boosterActionContext);
    }
}