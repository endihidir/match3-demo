using System;
using Core.Extensions;
using Core.Item;
using UnityEngine;

namespace Core.Config
{
    [Serializable]
    public struct BoosterMergeKey : IEquatable<BoosterMergeKey>
    {
        [field: SerializeField] public BoosterFamily First { get; private set; }
        [field: SerializeField] public BoosterFamily Second { get; private set; }

        public static BoosterMergeKey Create(BoosterType a, BoosterType b)
        {
            var fa = a.ToFamily();
            var fb = b.ToFamily();

            return (int)fa <= (int)fb ? new BoosterMergeKey { First = fa, Second = fb } : new BoosterMergeKey { First = fb, Second = fa };
        }

        public bool Equals(BoosterMergeKey other) => First == other.First && Second == other.Second;
    }
}