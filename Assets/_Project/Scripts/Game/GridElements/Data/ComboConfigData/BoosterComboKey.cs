using System;
using Core.Extensions;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public struct BoosterComboKey : IEquatable<BoosterComboKey>
    {
        [field: SerializeField] public BoosterFamily First { get; private set; }
        [field: SerializeField] public BoosterFamily Second { get; private set; }

        public static BoosterComboKey Create(BoosterType a, BoosterType b)
        {
            var fa = a.ToFamily();
            var fb = b.ToFamily();

            return (int)fa <= (int)fb ? new BoosterComboKey { First = fa, Second = fb } : new BoosterComboKey { First = fb, Second = fa };
        }

        public bool Equals(BoosterComboKey other) => First == other.First && Second == other.Second;
    }
}