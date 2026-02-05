using System;

namespace Core.Handlers
{
    public struct AnimationTimeline
    {
        private float[] _endTimeByColumn;
        private int[] _usedColumns;
        private int[] _usedStamp;
        private int _stampId;
        private int _usedCount;

        public void EnsureCapacity(int width)
        {
            if (_endTimeByColumn == null || _endTimeByColumn.Length < width)
            {
                _endTimeByColumn = new float[width];
                _usedColumns = new int[width];
                _usedStamp = new int[width];
            }
        }

        public void Reset(int width)
        {
            EnsureCapacity(width);
            
            for (int x = 0; x < width; x++)
                _endTimeByColumn[x] = 0f;
        }

        /// <summary>
        /// Marks a column as used for the current move and returns the count of marked columns.
        /// Call BeginCollect before marking, then use GetStartTime and UpdateEndTime.
        /// </summary>
        public void BeginCollect()
        {
            _stampId++;
            _usedCount = 0;

            if (_stampId == int.MaxValue)
            {
                Array.Clear(_usedStamp, 0, _usedStamp.Length);
                _stampId = 1;
            }
        }

        public void MarkColumn(int x)
        {
            if (x < 0 || x >= _endTimeByColumn.Length) return;
            if (_usedStamp[x] == _stampId) return;

            _usedStamp[x] = _stampId;
            _usedColumns[_usedCount++] = x;
        }

        public float GetStartTime()
        {
            var start = 0f;
            for (int i = 0; i < _usedCount; i++)
            {
                var t = _endTimeByColumn[_usedColumns[i]];
                if (t > start) start = t;
            }
            return start;
        }

        public void UpdateEndTime(float endTime)
        {
            for (int i = 0; i < _usedCount; i++)
                _endTimeByColumn[_usedColumns[i]] = endTime;
        }

        // Single column shortcut (for simple fall)
        public float GetColumnTime(int x) => _endTimeByColumn[x];
        public void SetColumnTime(int x, float time) => _endTimeByColumn[x] = time;
    }
}