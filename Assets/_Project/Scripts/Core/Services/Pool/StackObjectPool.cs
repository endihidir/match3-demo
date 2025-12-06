using System.Collections.Generic;

namespace Core.Pool
{
    public abstract class StackObjectPool<TBase, TData> where TBase : class where TData : struct
    {
        private Stack<TBase> _stack;
        private int _maxSize;

        public int Count => _stack.Count;

        public void Initialize(int initialCapacity = 0, int maxSize = int.MaxValue)
        {
            if (initialCapacity < 0) initialCapacity = 0;
            
            if (maxSize < 1) maxSize = int.MaxValue;

            _stack = new Stack<TBase>(initialCapacity);
            
            _maxSize = maxSize;

            for (int i = 0; i < initialCapacity; i++)
            {
                var item = CreateInstance(default);
                
                if (item != null)
                {
                    _stack.Push(item);
                }
            }
        }

        public TBase Get(TData data)
        {
            var item = _stack.Count > 0 ? _stack.Pop() : CreateInstance(data);

            OnGet(item);
            
            return item;
        }

        public void Release(TBase item)
        {
            if (item == null) return;

            if (_stack.Count >= _maxSize)
            {
                OnDestroy(item);
                return;
            }

            OnRelease(item);
            
            _stack.Push(item);
        }

        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var item = _stack.Pop();
                OnDestroy(item);
            }
        }

        protected abstract TBase CreateInstance(TData data);

        protected virtual void OnGet(TBase item)
        {
            
        }

        protected virtual void OnRelease(TBase item)
        {
        }

        protected virtual void OnDestroy(TBase item)
        {
            
        }
    }
}