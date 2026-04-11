using System.Collections.Generic;
using UnityEngine;

namespace Game.Grid.States.Data
{
    public class BoosterImpactTimeline
    {
        private readonly List<BoosterImpactEntry> _entries = new();
        public IReadOnlyList<BoosterImpactEntry> Entries => _entries;
 
        public void Add(Vector2Int coord, float delay) => _entries.Add(new BoosterImpactEntry(coord, delay));
 
        public void OffsetDelays(float offset)
        {
            if (offset <= 0f) return;
 
            for (int i = 0; i < _entries.Count; i++)
                _entries[i] = new BoosterImpactEntry(_entries[i].Coord, _entries[i].Delay + offset);
        }
 
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