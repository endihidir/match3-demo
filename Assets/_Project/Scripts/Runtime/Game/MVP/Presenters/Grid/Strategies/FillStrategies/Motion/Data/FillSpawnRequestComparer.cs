using System.Collections.Generic;

namespace Game.Grid.Strategies.Data
{
    public sealed class FillSpawnRequestComparer : IComparer<FillSpawnRequest>
    {
        public int Compare(FillSpawnRequest a, FillSpawnRequest b)
        {
            var column = a.Column.CompareTo(b.Column);
            return column != 0 ? column : a.StackIndex.CompareTo(b.StackIndex);
        }
    }
}