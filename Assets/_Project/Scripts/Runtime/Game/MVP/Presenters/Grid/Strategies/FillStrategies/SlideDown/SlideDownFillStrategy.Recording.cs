using System;
using Game.Grid.Item;
using Game.Grid.Strategies.Data;
using UnityEngine;

namespace Game.Grid.Strategies
{
    public sealed partial class SlideDownFillStrategy
    {
        // Records & path nodes
        // =========================================================

        private void AddStep(BaseGridObject item, Vector2Int step, bool isSpawn)
        {
            if (!_recordIndexByItem.TryGetValue(item, out var index))
            {
                index = _recordCount;
                _recordIndexByItem[item] = index;

                EnsureRecordCapacity(_recordCount + 1);
                _records[_recordCount++] = new SlideDownMoveRecord(item);
            }

            ref var record = ref _records[index];

            record.IsSpawn |= isSpawn;

            // Avoid pushing the same coord twice in a row
            if (record.TailNode >= 0 && _pathCoord[record.TailNode] == step)
            {
                record.FinalCoord = step;
                return;
            }

            var node = AllocPathNode(step);

            if (record.HeadNode < 0)
            {
                record.HeadNode = node;
            }
            else
            {
                _pathNext[record.TailNode] = node;
            }

            record.TailNode = node;
            record.PathCount++;
            record.FinalCoord = step;
        }

        private void EnsureRecordCapacity(int need)
        {
            if (_records.Length >= need) return;

            var newSize = _records.Length == 0 ? 256 : _records.Length * 2;
            if (newSize < need) newSize = need;

            Array.Resize(ref _records, newSize);
        }

        private int AllocPathNode(Vector2Int coord)
        {
            if (_pathNodeCount >= _pathCoord.Length)
            {
                var newSize = _pathCoord.Length == 0 ? 512 : _pathCoord.Length * 2;
                Array.Resize(ref _pathCoord, newSize);
                Array.Resize(ref _pathNext, newSize);
            }

            var idx = _pathNodeCount++;
            _pathCoord[idx] = coord;
            _pathNext[idx] = -1;
            return idx;
        }
    }
}