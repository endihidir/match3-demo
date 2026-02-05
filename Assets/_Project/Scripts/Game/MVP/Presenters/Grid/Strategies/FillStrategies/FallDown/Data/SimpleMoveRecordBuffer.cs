using System;

namespace Core.Handlers
{
    public struct SimpleMoveRecordBuffer<TRecord> where TRecord : struct
    {
        private TRecord[] _records;
        private int _count;

        private const int DefaultCapacity = 256;

        public int Count => _count;
        public TRecord[] Records => _records;

        public void EnsureCapacity(int capacity)
        {
            if (_records == null || _records.Length < capacity)
            {
                var newSize = _records?.Length ?? DefaultCapacity;
                while (newSize < capacity) newSize *= 2;
                Array.Resize(ref _records, newSize);
            }
        }

        public void Reset()
        {
            _count = 0;
        }

        public void Add(in TRecord record)
        {
            EnsureCapacity(_count + 1);
            _records[_count++] = record;
        }

        public ref readonly TRecord GetReadonly(int index) => ref _records[index];
    }
}