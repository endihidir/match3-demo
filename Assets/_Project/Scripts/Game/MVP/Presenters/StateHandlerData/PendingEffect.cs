using Core.Config;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct PendingEffect
    {
        public readonly Vector2Int Origin;
        public readonly BoosterActionBase boosterAction;

        public PendingEffect(Vector2Int origin, BoosterActionBase boosterAction)
        {
            Origin = origin;
            this.boosterAction = boosterAction;
        }
    }
}