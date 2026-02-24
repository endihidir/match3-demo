using System.Collections.Generic;

namespace  Game.Grid.Strategies.Data
{
    public readonly struct FallMoveOrderComparer : IComparer<int>
    {
        private readonly FallDownMoveRecord[] _records;

        public FallMoveOrderComparer(FallDownMoveRecord[] records)
        {
            _records = records;
        }

        public int Compare(int a, int b)
        {
            ref readonly var ra = ref _records[a];
            ref readonly var rb = ref _records[b];

            var y = rb.FinalCoord.y.CompareTo(ra.FinalCoord.y);
            if (y != 0) return y;

            var x = ra.FinalCoord.x.CompareTo(rb.FinalCoord.x);
            if (x != 0) return x;

            if (ra.IsSpawn != rb.IsSpawn) return ra.IsSpawn ? 1 : -1;
            return a.CompareTo(b);
        }
    }
}