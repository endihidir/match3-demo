using System;
using Core.Config;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct BoosterActionKey : IEquatable<BoosterActionKey>
    {
        private readonly Vector2Int _origin;
        private readonly Type _actionType;

        public BoosterActionKey(Vector2Int origin, BoosterActionBase action)
        {
            _origin = origin;
            _actionType = action.GetType();
        }

        public bool Equals(BoosterActionKey other) => _origin == other._origin && _actionType == other._actionType;
        public override bool Equals(object obj) => obj is BoosterActionKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_origin, _actionType);
    }
}