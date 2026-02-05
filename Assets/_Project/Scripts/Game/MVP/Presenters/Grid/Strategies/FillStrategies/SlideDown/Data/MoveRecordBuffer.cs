using System;
using System.Collections.Generic;

namespace Core.Handlers
{
    public struct MoveRecordBuffer<TRecord, TKey> where TRecord : struct
    {
        private TRecord[] _records;
        private int _count;
        private Dictionary<TKey, int> _indexByKey;
        private readonly bool _useKeyLookup;

        private const int DefaultCapacity = 256;

        public int Count => _count;
        public TRecord[] Records => _records;

        public MoveRecordBuffer(bool useKeyLookup)
        {
            _records = null;
            _count = 0;
            _indexByKey = useKeyLookup ? new Dictionary<TKey, int>(256) : null;
            _useKeyLookup = useKeyLookup;
        }

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
            _indexByKey?.Clear();
        }

        public int Add(in TRecord record)
        {
            EnsureCapacity(_count + 1);
            var index = _count++;
            _records[index] = record;
            return index;
        }

        public int AddOrGet(TKey key, in TRecord defaultRecord)
        {
            if (!_useKeyLookup)
                throw new InvalidOperationException("Key lookup not enabled for this buffer");

            if (_indexByKey.TryGetValue(key, out var index))
                return index;

            index = Add(defaultRecord);
            _indexByKey[key] = index;
            return index;
        }

        public bool TryGetIndex(TKey key, out int index)
        {
            if (_useKeyLookup && _indexByKey != null)
                return _indexByKey.TryGetValue(key, out index);

            index = -1;
            return false;
        }

        public ref TRecord GetRef(int index) => ref _records[index];
        public ref readonly TRecord GetReadonly(int index) => ref _records[index];
    }
}