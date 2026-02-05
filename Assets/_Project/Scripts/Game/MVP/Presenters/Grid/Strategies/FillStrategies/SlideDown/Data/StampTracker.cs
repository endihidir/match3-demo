using System;

namespace Core.Handlers
{
    /// <summary>
    /// Efficient stamp-based tracking to avoid repeated processing.
    /// Used for marking cells/columns as "already processed this pass".
    /// </summary>
    public struct StampTracker
    {
        private int[] _stamps;
        private int _currentStamp;

        public void EnsureCapacity(int capacity)
        {
            if (_stamps == null || _stamps.Length < capacity)
                _stamps = new int[capacity];
        }

        public void NextPass()
        {
            _currentStamp++;

            if (_currentStamp == int.MaxValue)
            {
                Array.Clear(_stamps, 0, _stamps.Length);
                _currentStamp = 1;
            }
        }

        public bool TryMark(int index)
        {
            if (index < 0 || index >= _stamps.Length) return false;
            if (_stamps[index] == _currentStamp) return false;

            _stamps[index] = _currentStamp;
            return true;
        }

        public bool IsMarked(int index)
        {
            if (index < 0 || index >= _stamps.Length) return false;
            return _stamps[index] == _currentStamp;
        }

        public void Reset()
        {
            // Clear the array to avoid false positives from previous runs
            if (_stamps != null)
                Array.Clear(_stamps, 0, _stamps.Length);
            
            _currentStamp = 0;
        }
    }
}