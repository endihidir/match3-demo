using System;
using Core.Configs;
using UnityEngine;

namespace Core.Handlers
{
    public readonly struct BoosterActionKey : IEquatable<BoosterActionKey>
    {
        public readonly Vector2Int Origin;
        public readonly BoosterActionBase Action;

        public BoosterActionKey(Vector2Int origin, BoosterActionBase action)
        {
            Origin = origin;
            Action = action;
        }

        public bool Equals(BoosterActionKey other) => Origin == other.Origin && Action == other.Action;
        public override int GetHashCode() => HashCode.Combine(Origin, Action);
    }
}