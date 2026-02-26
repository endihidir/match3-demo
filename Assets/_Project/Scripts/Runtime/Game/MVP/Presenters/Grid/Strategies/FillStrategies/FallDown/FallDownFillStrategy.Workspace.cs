using System;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy
    {
        private FallDownMoveRecord[] _records = new FallDownMoveRecord[256];
        private int _recordCount;

        private float[] _timelineByX = Array.Empty<float>();
        private int[] _order = Array.Empty<int>();
        private UniTask[] _animTasks = new UniTask[128];
        private UniTask _pendingAnimations = UniTask.CompletedTask;

        /// <summary>
        /// Single entry point called at the top of every Execute().
        /// Resets counters and ensures all arrays are large enough.
        /// </summary>
        private void PrepareForRun(int width, int height)
        {
            _recordCount = 0;

            // Column-indexed arrays.
            EnsureSize(ref _timelineByX, width);
            Array.Clear(_timelineByX, 0, width);

            // Record + sort-order capacity: worst case every cell has a record.
            var cellCount = width * height;
            EnsureRecordCapacity(cellCount);
            EnsureSize(ref _order, cellCount);
        }

        private void AddRecord(in FallDownMoveRecord record)
        {
            // Capacity is pre-ensured in PrepareForRun; this is a safety net
            // in case a grid-size edge case exceeds the estimate.
            if (_recordCount == _records.Length)
                Array.Resize(ref _records, _records.Length * 2);

            _records[_recordCount++] = record;
        }

        private void EnsureRecordCapacity(int minCount)
        {
            if (_records.Length < minCount)
                Array.Resize(ref _records, Math.Max(_records.Length * 2, minCount));
        }

        private static void EnsureSize<T>(ref T[] array, int minLength)
        {
            if (array.Length >= minLength) return;
            var newSize = Math.Max(array.Length == 0 ? 16 : array.Length * 2, minLength);
            Array.Resize(ref array, newSize);
        }
    }
}