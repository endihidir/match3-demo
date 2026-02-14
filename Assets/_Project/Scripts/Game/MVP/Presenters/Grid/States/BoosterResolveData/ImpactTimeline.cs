using System.Collections.Generic;
using UnityEngine;

namespace Core.Handlers
{
    public class ImpactTimeline
    {
        private readonly List<ImpactEntry> _entries = new();
        public IReadOnlyList<ImpactEntry> Entries => _entries;
        public void Add(Vector2Int coord, float delay) => _entries.Add(new ImpactEntry(coord, delay));
        public void SortByDelay() => _entries.Sort((a, b) => a.Delay.CompareTo(b.Delay));
        
        public void SortByDelayThenCoord()
        {
            _entries.Sort((a, b) =>
            {
                var delayCompare = a.Delay.CompareTo(b.Delay);
                if (delayCompare != 0) return delayCompare;
        
                var xCompare = a.Coord.x.CompareTo(b.Coord.x);
                if (xCompare != 0) return xCompare;
     
                return a.Coord.y.CompareTo(b.Coord.y);
            });
        }
    }
}