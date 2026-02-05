using System;
using Cysharp.Threading.Tasks;

namespace Core.Handlers
{
    public struct AnimationTaskCollector
    {
        private UniTask[] _tasks;
        private int _count;

        private const int DefaultCapacity = 128;

        public int Count => _count;

        public void EnsureCapacity(int capacity)
        {
            if (_tasks == null || _tasks.Length < capacity)
            {
                var newSize = _tasks?.Length ?? DefaultCapacity;
                while (newSize < capacity) newSize *= 2;
                Array.Resize(ref _tasks, newSize);
            }
        }

        public void Reset()
        {
            _count = 0;
        }

        public void Add(UniTask task)
        {
            EnsureCapacity(_count + 1);
            _tasks[_count++] = task;
        }

        public UniTask WhenAll()
        {
            if (_count == 0)
                return UniTask.CompletedTask;

            // Resize to exact count for WhenAll
            if (_tasks.Length != _count)
                Array.Resize(ref _tasks, _count);

            return UniTask.WhenAll(_tasks);
        }
    }
}