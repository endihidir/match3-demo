using System.Collections.Generic;

namespace Core.Handlers
{
    public readonly struct SlideMoveOrderComparer : IComparer<int>
    {
        private readonly SlideDownMoveRecord[] _records;

        public SlideMoveOrderComparer(SlideDownMoveRecord[] records)
        {
            _records = records;
        }

        public int Compare(int a, int b)
        {
            ref readonly var ra = ref _records[a];
            ref readonly var rb = ref _records[b];

            // Primary: lower items first (higher y first while scanning from bottom visually).
            var y = rb.FinalCoord.y.CompareTo(ra.FinalCoord.y);
            if (y != 0) return y;

            var x = ra.FinalCoord.x.CompareTo(rb.FinalCoord.x);
            if (x != 0) return x;

            // Non-spawn before spawn in the same target.
            if (ra.IsSpawn != rb.IsSpawn) return ra.IsSpawn ? 1 : -1;

            // Falls before slides when everything else ties.
            if (ra.IsSlide != rb.IsSlide) return ra.IsSlide ? 1 : -1;

            return a.CompareTo(b);
        }
    }
}