using Core.Configs;
using UnityEngine;

namespace Core.Item
{
    public interface IBoosterActionSource
    {
        BoosterActionBase BoosterAction { get; }
        bool TryBuildAction(Vector2Int origin, out BoosterActionContext boosterActionContext);
    }
}