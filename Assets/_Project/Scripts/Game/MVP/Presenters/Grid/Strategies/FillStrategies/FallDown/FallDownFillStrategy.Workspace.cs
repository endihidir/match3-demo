using System;
using Cysharp.Threading.Tasks;
using Game.Grid.Strategies.Data;

namespace Game.Grid.Strategies
{
    public sealed partial class FallDownFillStrategy
    {
        private FallDownMoveRecord[] _records = Array.Empty<FallDownMoveRecord>();
        private int _recordCount;

        private float[] _timelineByX = Array.Empty<float>();
        private int[] _order = Array.Empty<int>();
        private UniTask[] _animTasks = new UniTask[128];
        private UniTask _runningAnimations;

        private void EnsureBuffers(int width)
        {
            if (_timelineByX.Length < width)
                _timelineByX = new float[width];

            if (_order.Length < _recordCount)
                Array.Resize(ref _order, _recordCount);
        }

        private void EnsureRecordCapacity(int capacity)
        {
            if (_records.Length < capacity)
                Array.Resize(ref _records, capacity);
        }

        private void AddRecord(in FallDownMoveRecord record)
        {
            if (_recordCount >= _records.Length)
                Array.Resize(ref _records, _records.Length == 0 ? 256 : _records.Length * 2);

            _records[_recordCount++] = record;
        }
    }
}