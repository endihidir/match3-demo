using System;
using Game.Extensions;
using Game.Grid.Item;
using UnityEngine;

namespace Game.Configs
{
    [Serializable]
    public struct BoosterComboKey : IEquatable<BoosterComboKey>
    {
        [field: SerializeField] public BoosterFamily First { get; private set; }
        [field: SerializeField] public BoosterFamily Second { get; private set; }

        public static BoosterComboKey Create(BoosterType boosterTypeA, BoosterType boosterTypeB)
        {
            var fa = boosterTypeA.ToFamily();
            var fb = boosterTypeB.ToFamily();

            return (int)fa <= (int)fb ? new BoosterComboKey { First = fa, Second = fb } : new BoosterComboKey { First = fb, Second = fa };
        }

        public bool Equals(BoosterComboKey other) => First == other.First && Second == other.Second;
    }
}