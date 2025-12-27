using Core.Config;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct PendingEffect
    {
        public readonly Vector2Int OriginCoord;
        public readonly BoosterActionBase BoosterAction;

        public PendingEffect(Vector2Int originCoord, BoosterActionBase boosterAction)
        {
            OriginCoord = originCoord;
            BoosterAction = boosterAction;
        }
    }
}