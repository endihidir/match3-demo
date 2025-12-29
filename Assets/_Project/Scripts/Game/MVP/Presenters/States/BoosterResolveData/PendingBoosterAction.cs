using Core.Config;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct PendingBoosterAction
    {
        public readonly Vector2Int OriginCoord;
        public readonly BoosterActionBase BoosterAction;

        public PendingBoosterAction(Vector2Int originCoord, BoosterActionBase boosterAction)
        {
            OriginCoord = originCoord;
            BoosterAction = boosterAction;
        }
    }
}