using Game.Grid.Item;
using Game.Grid.Strategies.Data;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        /// <summary>
        /// Records a single position step for <paramref name="item"/>.
        /// Creates a new record on first call for a given item.
        /// Consecutive duplicate coordinates are collapsed (no-op move guard).
        /// </summary>
        private void AddStep(BaseGridObject item, Vector2Int coord, bool isSpawn)
        {
            if (!_recordByItem.TryGetValue(item, out var idx))
            {
                EnsureRecordCapacity(_recordCount + 1);
                idx = _recordCount++;
                _recordByItem[item] = idx;
                _records[idx] = new SlideDownMoveRecord(item);
            }

            ref var record = ref _records[idx];

            // Mark spawn regardless of how many steps are added.
            record.IsSpawn |= isSpawn;

            // Collapse consecutive identical coordinates.
            if (record.TailNode >= 0 && _pathCoord[record.TailNode] == coord)
            {
                record.FinalCoord = coord;
                return;
            }

            var node = AllocNode(coord);

            if (record.HeadNode < 0)
                record.HeadNode = node;
            else
                _pathNext[record.TailNode] = node;

            record.TailNode   = node;
            record.PathCount++;
            record.FinalCoord = coord;
        }
    }
}