using System;
using System.Collections.Generic;

namespace Core.Pool
{
    public abstract class PolymorphicPool<TBase, TData> where TBase : class where TData : struct
    {
        private readonly Dictionary<Type, Stack<TBase>> _stacks = new();

        public int GetCount<TConcrete>() where TConcrete : class, TBase
        {
            var type = typeof(TConcrete);
            return _stacks.TryGetValue(type, out var stack) ? stack.Count : 0;
        }

        public TConcrete Get<TConcrete>(TData data) where TConcrete : class, TBase
        {
            var type = typeof(TConcrete);

            if (!_stacks.TryGetValue(type, out var stack))
            {
                stack = new Stack<TBase>();
                _stacks[type] = stack;
            }

            var item = stack.Count > 0 ? stack.Pop() : CreateInstance<TConcrete>(data);

            OnGet(item);
            return (TConcrete)item;
        }

        public void Release<TConcrete>(TConcrete item) where TConcrete : class, TBase
        {
            if (item == null) return;

            var type = typeof(TConcrete);

            if (!_stacks.TryGetValue(type, out var stack))
            {
                stack = new Stack<TBase>();
                _stacks[type] = stack;
            }

            OnRelease(item);
            stack.Push(item);
        }

        public void Clear()
        {
            foreach (var stack in _stacks.Values)
            {
                while (stack.Count > 0)
                {
                    var item = stack.Pop();
                    OnDestroy(item);
                }
            }

            _stacks.Clear();
        }
        
        protected abstract T CreateInstance<T>(TData data) where T : class, TBase;

        protected virtual void OnGet(TBase item) { }
        protected virtual void OnRelease(TBase item) { }
        protected virtual void OnDestroy(TBase item) { }
    }
}