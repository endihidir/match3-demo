using Core.Config;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct PendingEffect
    {
        public readonly Vector2Int Origin;
        public readonly BoosterEffectBase BoosterEffect;

        public PendingEffect(Vector2Int origin, BoosterEffectBase boosterEffect)
        {
            Origin = origin;
            BoosterEffect = boosterEffect;
        }
    }
}