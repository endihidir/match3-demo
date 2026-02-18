using Game.Configs;
using Game.Grid.Contexts;
using UnityEngine;

namespace Game.Grid.Item
{
    public interface IBoosterActionSource
    {
        BoosterActionBase BoosterAction { get; }
        bool TryBuildAction(Vector2Int origin, out BoosterActionContext boosterActionContext);
    }
}