using Game.Configs;
using UnityEngine;

namespace Game.Grid.Contexts
{
    public readonly struct BoosterActionContext
    {
        public readonly Vector2Int OriginCoord;
        public readonly BoosterActionBase BoosterAction;

        public BoosterActionContext(Vector2Int originCoord, BoosterActionBase boosterAction)
        {
            OriginCoord = originCoord;
            BoosterAction = boosterAction;
        }
    }
}