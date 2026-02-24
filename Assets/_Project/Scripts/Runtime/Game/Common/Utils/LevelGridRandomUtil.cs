using System;
using Game.Grid.Item;

namespace Game.Utils
{
    public static class LevelGridRandomUtil
    {
        private static int[] _cachedItemTypeIds;

        public static Random CreateRng(int levelNumber, bool useSeededPattern, int seedOverride)
        {
            if (!useSeededPattern) return null;

            var seed = seedOverride != 0 ? seedOverride : GetSeedForLevel(levelNumber);
            return new Random(seed);
        }

        public static int GetRandomItemTypeId(Random rng)
        {
            var ids = GetCachedItemTypeIds();
            return ids[NextIndex(rng, ids.Length)];
        }

        public static int NextIndex(Random rng, int maxExclusive)
        {
            return rng?.Next(0, maxExclusive) ?? UnityEngine.Random.Range(0, maxExclusive);
        }
        
        public static TEnum GetRandomEnumValue<TEnum>(int firstIndex = 0) where TEnum : Enum
        {
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
            return values[UnityEngine.Random.Range(firstIndex, values.Length)];
        }

        private static int[] GetCachedItemTypeIds()
        {
            if (_cachedItemTypeIds != null)
                return _cachedItemTypeIds;

            var values = (ItemType[])Enum.GetValues(typeof(ItemType));

            var count = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if ((int)values[i] > 0)
                    count++;
            }

            var ids = new int[count];
            var write = 0;

            for (int i = 0; i < values.Length; i++)
            {
                var id = (int)values[i];
                if (id > 0)
                    ids[write++] = id;
            }

            _cachedItemTypeIds = ids;
            return _cachedItemTypeIds;
        }

        private static int GetSeedForLevel(int levelNumber)
        {
            var x = levelNumber;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;

            if (x == 0) x = 1337;
            return x;
        }
        
        public static int[] GetItemTypeIds() => GetCachedItemTypeIds();
    }
}