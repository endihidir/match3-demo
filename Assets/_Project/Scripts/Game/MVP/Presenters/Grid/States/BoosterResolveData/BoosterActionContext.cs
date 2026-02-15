using Core.Configs;
using UnityEngine;

namespace Core.Handlers
{
    public struct BoosterActionContext
    {
        public Vector2Int OriginCoord { get; private set; }
        public BoosterActionBase BoosterAction { get; private set; }

        public BoosterActionContext(Vector2Int originCoord, BoosterActionBase boosterAction)
        {
            OriginCoord = originCoord;
            BoosterAction = boosterAction;
        }
    }
}