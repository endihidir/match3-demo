using System;

namespace Core.Handlers
{
    public struct SortOrderHelper
    {
        private int[] _order;

        public int[] Order => _order;

        public void Prepare(int count)
        {
            if (_order == null || _order.Length < count)
                _order = new int[count];

            for (int i = 0; i < count; i++)
                _order[i] = i;
        }

        public void Sort<TComparer>(int count, TComparer comparer) where TComparer : System.Collections.Generic.IComparer<int>
        {
            Array.Sort(_order, 0, count, comparer);
        }
    }
}