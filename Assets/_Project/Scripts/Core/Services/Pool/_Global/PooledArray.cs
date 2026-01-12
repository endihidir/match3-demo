using System;
using System.Buffers;

namespace Core.Pool
{
    public readonly struct PooledArray<T> : IDisposable
    {
        public readonly T[] Array;
        public readonly int Length;

        public PooledArray(int length)
        {
            Length = length;
            Array = ArrayPool<T>.Shared.Rent(length);
        }

        public void Dispose() => ArrayPool<T>.Shared.Return(Array);
    }
}