using System;
using UnityEngine;

namespace Core.Handlers
{
    public struct PathNodePool
    {
        private Vector2Int[] _coords;
        private int[] _next;
        private int _count;

        private const int DefaultCapacity = 512;

        public int Count => _count;

        public void EnsureCapacity(int capacity)
        {
            if (_coords == null || _coords.Length < capacity)
            {
                var newSize = _coords?.Length ?? DefaultCapacity;
                while (newSize < capacity) newSize *= 2;

                Array.Resize(ref _coords, newSize);
                Array.Resize(ref _next, newSize);
            }
        }

        public void Reset()
        {
            _count = 0;
        }

        public int Allocate(Vector2Int coord)
        {
            EnsureCapacity(_count + 1);

            var index = _count++;
            _coords[index] = coord;
            _next[index] = -1;
            return index;
        }

        public void Link(int fromNode, int toNode)
        {
            if (fromNode >= 0 && fromNode < _count)
                _next[fromNode] = toNode;
        }

        public Vector2Int GetCoord(int node)
        {
            return node >= 0 && node < _count ? _coords[node] : default;
        }

        public int GetNext(int node)
        {
            return node >= 0 && node < _count ? _next[node] : -1;
        }

        // Direct array access for animation schedulers (avoid copying)
        public Vector2Int[] CoordArray => _coords;
        public int[] NextArray => _next;
    }
}