using System;
using Core.Config;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct EffectKey : IEquatable<EffectKey>
    {
        private readonly Vector2Int _origin;
        private readonly Type _actionType;

        public EffectKey(Vector2Int origin, BoosterActionBase action)
        {
            _origin = origin;
            _actionType = action.GetType();
        }

        public bool Equals(EffectKey other) => _origin == other._origin && _actionType == other._actionType;
        public override bool Equals(object obj) => obj is EffectKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_origin, _actionType);
    }
}